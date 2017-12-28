using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Topshelf.Logging;
using ZKTeco.SyncBackendService.Bases;
using ZKTeco.SyncBackendService.Events;
using ZKTeco.SyncBackendService.Models;

namespace ZKTeco.SyncBackendService.Connectors
{
    public class SqliteConnector : ServiceBase
    {
        const string SYNCDB = "syncdb.sqlite";

        private static SQLiteConnectionStringBuilder _builder;

        private SQLiteConnection _connection;

        public SqliteConnector()
        {
            InitDbBuilder();
            _connection = new SQLiteConnection(_builder.ConnectionString);

            Logger = HostLogger.Get<SqliteConnector>();           
        }

        public void Enqueue(AttendanceLog model)
        {
            var message = JsonConvert.SerializeObject(model);
            try
            {
                if (_connection.State == System.Data.ConnectionState.Closed)
                {
                    _connection.Open();
                }

                var command = new SQLiteCommand(_connection);
                command.CommandText = "INSERT INTO queue(refer_id, message) VALUES(@refer_id, @message);";
                command.Parameters.Add(new SQLiteParameter("@refer_id", model.Id));
                command.Parameters.Add(new SQLiteParameter("@message", message));
                command.ExecuteNonQuery();
            }
            catch (SQLiteException se)
            {
                Logger.ErrorFormat("Enqueue error:{@ex}", se);
                throw;
            }

            EventHub.Instance.PublishAsync(new EventMessage(EventType.SyncWeb, model.Id, message));
        }

        #region Attendance log

        public bool HasSameAttandanceLog(long id)
        {
            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                _connection.Open();
            }

            var command = new SQLiteCommand(_connection);
            command.CommandText = "SELECT IFNULL(COUNT(*), 0) AS num FROM attendance_logs WHERE id=@id;";
            command.Parameters.Add(new SQLiteParameter("@id", System.Data.DbType.Int64) { Value = id });
            var result = command.ExecuteScalar(System.Data.CommandBehavior.SingleResult);
            if (result == null)
            {
                return false;
            }
            return ((int)result > 0);
        }

        public List<AttendanceLog> GetUnsyncAttendanceLogs()
        {
            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                _connection.Open();
            }

            var logs = new List<AttendanceLog>(10);

            var command = new SQLiteCommand(_connection);
            command.CommandText = @"SELECT id,enroll_number,state,mode,log_date,work_code,
                machine_id,project_id,ifnull(device_name,''), ifnull(device_type,0) FROM attendance_logs 
                WHERE sync=0;";            
            var results = command.ExecuteReader();
            if (results == null)
            {
                return logs;
            }

            while(results.Read())
            {
                logs.Add(new AttendanceLog(
                    results.GetInt64(0),                    
                    results.GetString(1),
                    results.GetInt32(2),
                    results.GetInt32(3),
                    results.GetDateTime(4),
                    results.GetInt32(5),
                    results.GetInt32(6),
                    results.GetString(7),
                    results.GetString(8),
                    (DeviceType)results.GetInt32(9)
                    ));
            }

            return logs;
        }

        public List<AttendanceLog> GetAttendanceLogsWithinDate(string enrollNumber, DateTime date)
        {
            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                _connection.Open();
            }

            var logs = new List<AttendanceLog>(10);

            var command = new SQLiteCommand(_connection);
            command.CommandText = @"SELECT id, enroll_number, state, mode, log_date, work_code,
                machine_id, project_id, ifnull(device_name,''), ifnull(device_type,0) FROM attendance_logs 
                WHERE enroll_number=@enroll_number AND log_date=@date;";
            command.Parameters.Add(new SQLiteParameter("@enroll_number", enrollNumber));
            command.Parameters.Add(new SQLiteParameter("@date", date.Date));
            var results = command.ExecuteReader();
            if (results == null)
            {
                return logs;
            }

            while (results.Read())
            {
                logs.Add(new AttendanceLog(
                    results.GetInt64(0),
                    results.GetString(1),
                    results.GetInt32(2),
                    results.GetInt32(3),
                    results.GetDateTime(4),
                    results.GetInt32(5),
                    results.GetInt32(6),
                    results.GetString(7),
                    results.GetString(8),
                    (DeviceType)results.GetInt32(9)
                    ));
            }

            return logs;
        }

        public void AddAttendanceLog(AttendanceLog model)
        {
            try
            {
                if (_connection.State == System.Data.ConnectionState.Closed)
                {
                    _connection.Open();
                }
                var command = new SQLiteCommand(_connection);
                command.CommandText = @"INSERT INTO attendance_logs(id,machine_id,enroll_number,
                    project_id,log_date,mode,state,work_code,sync,device_name,device_type) 
                    VALUES(@id, @machine_id, @enroll_number, @project_id, @log_date, @mode, 
                           @state, @work_code, 0,@device_name,@device_type);";
                command.Parameters.Add(new SQLiteParameter("@id", System.Data.DbType.Int64) { Value=model.Id });
                command.Parameters.Add(new SQLiteParameter("@machine_id", System.Data.DbType.Int32) { Value = model.MachineId });
                command.Parameters.Add(new SQLiteParameter("@enroll_number", System.Data.DbType.String) { Value = model.UserId });
                command.Parameters.Add(new SQLiteParameter("@project_id", System.Data.DbType.String) { Value = model.ProjectId });
                command.Parameters.Add(new SQLiteParameter("@log_date", System.Data.DbType.DateTime) { Value = model.LogDate });
                command.Parameters.Add(new SQLiteParameter("@mode", System.Data.DbType.Int32) { Value = model.Mode });
                command.Parameters.Add(new SQLiteParameter("@state", System.Data.DbType.Int32) { Value = model.State });
                command.Parameters.Add(new SQLiteParameter("@work_code", System.Data.DbType.Int32) { Value = model.WorkCode });
                command.Parameters.Add(new SQLiteParameter("@device_name", System.Data.DbType.String) { Value = model.DeviceName });
                command.Parameters.Add(new SQLiteParameter("@device_type", System.Data.DbType.Int32) { Value = model.DeviceType });
                command.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }           
        }

        public void SyncAttendanceLogSuccess(long id)
        {
            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                _connection.Open();
            }
            var command = new SQLiteCommand(_connection);
            command.CommandText = @"UPDATE attendance_logs SET sync=1 WHERE id=@id;";
            command.Parameters.Add(new SQLiteParameter("@id", System.Data.DbType.Int64) { Value = id });
            command.ExecuteNonQuery();
        }

        #endregion

        #region Workers

        public string GetWorkerIdByEnrollNumberAndProjectId(string enrollNumber, string projectId)
        {
            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                _connection.Open();
            }
            var command = new SQLiteCommand(_connection);
            command.CommandText = "SELECT ifnull(user_id,'') AS user FROM user_maps WHERE enroll_number=@enroll_number AND project_id=@project_id;";
            command.Parameters.Add(new SQLiteParameter("@enroll_number", enrollNumber));
            command.Parameters.Add(new SQLiteParameter("@project_id", projectId));
            var result = command.ExecuteScalar();
            if (result == DBNull.Value)
            {
                result = "";
            }

            return result.ToString();
        }

        public int AddWorker(WorkerInfo info)
        {
            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                _connection.Open();
            }
            var command = new SQLiteCommand(_connection);
            command.CommandText = "INSERT INTO user_maps(user_id,enroll_number,project_id) VALUES(@worker_id, @enroll_number, @project_id);";
            command.Parameters.Add(new SQLiteParameter("@worker_id", info.UserId));
            command.Parameters.Add(new SQLiteParameter("@enroll_number", info.EnrollNumber));
            command.Parameters.Add(new SQLiteParameter("@project_id", info.ProjectId));
            return command.ExecuteNonQuery(System.Data.CommandBehavior.KeyInfo);
        }

        #endregion

        #region install database

        public static void InstallSyncDatabase()
        {
            var dbPath = InitDbBuilder();
            if (!File.Exists(dbPath))
            {
                var dir = Path.GetDirectoryName(dbPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                SQLiteConnection.CreateFile(dbPath);
            }

            _builder = new SQLiteConnectionStringBuilder
            {
                DataSource = dbPath, Version = 3
            };

            CreateTableStructures(_builder);
        } 
        
        private static void CreateTableStructures(SQLiteConnectionStringBuilder builder)
        {            
            var connection = new SQLiteConnection(builder.ConnectionString);
            try
            {
                connection.Open();
                var tx = connection.BeginTransaction();
                
                var command = new SQLiteCommand(connection);
                CreateUserMapTable(command);
                CreateQueueTable(command);
                CreateAttendanceLogTable(command);
                CreateAttendanceLogArchiveTable(command);

                tx.Commit();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }                
            }            
        }        

        private static void CreateUserMapTable(SQLiteCommand command)
        {
            var sql = @"CREATE TABLE IF NOT EXISTS user_maps(
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            user_id        TEXT NOT NULL,
                            enroll_number  TEXT NOT NULL,
                            project_id     TEXT NOT NULL,
                            create_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            change_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
                        );";
            command.CommandText = sql;
            command.CommandType = System.Data.CommandType.Text;
            command.ExecuteNonQuery();
        }

        private static void CreateQueueTable(SQLiteCommand command)
        {
            var sql = @"CREATE TABLE IF NOT EXISTS queue(
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            refer_id INT NOT NULL,
                            message TEXT NOT NULL,
                            create_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                        );";
            command.CommandText = sql;
            command.CommandType = System.Data.CommandType.Text;
            command.ExecuteNonQuery();
        }

        private static void CreateAttendanceLogTable(SQLiteCommand command)
        {
            var sql = @"CREATE TABLE IF NOT EXISTS attendance_logs(
                            id INT PRIMARY KEY,
                            machine_id INT NOT NULL,
                            enroll_number  TEXT NOT NULL,
                            project_id     TEXT NOT NULL,
                            log_date       TIMESTAMP NOT NULL,
                            mode           INT NOT NULL,
                            state          INT NOT NULL,
                            work_code      INT NOT NULL,
                            device_name    TEXT NOT NULL,
                            device_type    INT NOT NULL,
                            create_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            change_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            sync           NUMERIC  NOT NULL
                        );";
            command.CommandText = sql;
            command.CommandType = System.Data.CommandType.Text;
            command.ExecuteNonQuery();
        }
        
        private static void CreateAttendanceLogArchiveTable(SQLiteCommand command)
        {
            var sql = @"CREATE TABLE IF NOT EXISTS attendance_logs_archive(
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            refer_no    INT NOT NULL,
                            machine_id INT NOT NULL,
                            enroll_number  TEXT NOT NULL,
                            project_id     TEXT NOT NULL,
                            log_date       TIMESTAMP NOT NULL,
                            mode           INT NOT NULL,
                            state          INT NOT NULL,
                            work_code      INT NOT NULL, 
                            device_name    TEXT NOT NULL,
                            device_type    INT NOT NULL,
                            create_at      TIMESTAMP  NOT NULL DEFAULT CURRENT_TIMESTAMP
                        );";
            command.CommandText = sql;
            command.CommandType = System.Data.CommandType.Text;
            command.ExecuteNonQuery();
        }

        #endregion

        public void Dispose()
        {
            if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
                _connection = null;
            }
        }

        private static string InitDbBuilder()
        {
            var dbPath = Path.Combine(ZKTecoConfig.AppRootFolder, "data", SYNCDB);
            _builder = new SQLiteConnectionStringBuilder
            {
                DataSource = dbPath,
                Version = 3
            };
            return dbPath;
        }

    }
}
