using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Topshelf.Logging;
using ZKTeco.SyncBackendService.Bases;
using ZKTeco.SyncBackendService.Events;
using ZKTeco.SyncBackendService.Models;
using ZKTeco.SyncBackendService.Utils;

namespace ZKTeco.SyncBackendService.Connectors
{
    public class SqliteConnector : ServiceBase
    {
        private SQLiteConnection _connection;

        public SqliteConnector()
        {            
            _connection = new SQLiteConnection(DbInstaller.ConnectionString);
            Logger = HostLogger.Get<SqliteConnector>();           
        }

        #region Queue

        public string Enqueue(long id, string message)
        {
            try
            {
                if (_connection.State == System.Data.ConnectionState.Closed)
                {
                    _connection.Open();
                }
                var guid = Guid.NewGuid();
                var command = new SQLiteCommand(_connection);
                command.CommandText = "INSERT INTO queue(id, refer_id, message) VALUES(@id, @refer_id, @message);";
                command.Parameters.Add(new SQLiteParameter("@id", guid));
                command.Parameters.Add(new SQLiteParameter("@refer_id", id));
                command.Parameters.Add(new SQLiteParameter("@message", message));
                command.ExecuteNonQuery();
                return guid.ToString("D");
            }
            catch (SQLiteException se)
            {
                Logger.ErrorFormat("Enqueue error:{@ex}", se);
                throw;
            }
        }

        public void Dequeue(string id)
        {
            try
            {
                if (_connection.State == System.Data.ConnectionState.Closed)
                {
                    _connection.Open();
                }                
                var command = new SQLiteCommand(_connection);
                command.CommandText = "DELETE FROM queue WHERE id=@id;";
                command.Parameters.Add(new SQLiteParameter("@id", id));
                command.ExecuteNonQuery();
            }
            catch (SQLiteException se)
            {
                Logger.ErrorFormat("Enqueue error:{@ex}", se);
                throw;
            }
        }

        #endregion

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
                machine_id,project_id,ifnull(device_name,''), ifnull(device_type,0), log_status FROM attendance_logs 
                WHERE sync=-1;";

            SQLiteDataReader read = null;

            try
            {
                read = command.ExecuteReader();
                while (read.Read())
                {
                    logs.Add(new AttendanceLog(
                        read.GetInt64(0),
                        read.GetString(1),
                        read.GetInt32(2),
                        read.GetInt32(3),
                        read.GetDateTime(4),
                        read.GetInt32(5),
                        read.GetInt32(6),
                        read.GetString(7),
                        read.GetString(8),
                        (DeviceType)read.GetInt32(9),
                        (AttendanceStatus)read.GetInt32(10)
                        ));                    
                }
                return logs;
            }
            finally
            {
                if (read != null)
                {
                    read.Close();
                }
            }           
        }        

        public AttendanceLog GetLastAttendanceLogByEnrollNumber(string enrollNumber)
        {
            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                _connection.Open();
            }

            AttendanceLog log = null;

            var command = new SQLiteCommand(_connection);
            command.CommandText = @"SELECT id, enroll_number, state, mode, log_date, work_code,
                machine_id, project_id, ifnull(device_name,''), ifnull(device_type,0), log_status FROM attendance_logs 
                WHERE enroll_number=@enroll_number ORDER BY log_date DESC LIMIT 1;";
            command.Parameters.Add(new SQLiteParameter("@enroll_number", enrollNumber));
            SQLiteDataReader reader = null;
            try
            {                
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    log = new AttendanceLog(
                        reader.GetInt64(0),
                        reader.GetString(1),
                        reader.GetInt32(2),
                        reader.GetInt32(3),
                        reader.GetDateTime(4),
                        reader.GetInt32(5),
                        reader.GetInt32(6),
                        reader.GetString(7),
                        reader.GetString(8),
                        (DeviceType)reader.GetInt32(9),
                        (AttendanceStatus)reader.GetInt32(10)
                        );
                    break;
                }
                return log;                
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
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
                    project_id,log_date,mode,state,work_code,sync,device_name,device_type,log_status) 
                    VALUES(@id, @machine_id, @enroll_number, @project_id, @log_date, @mode, 
                           @state, @work_code, -1, @device_name,@device_type,@log_status);";
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
                command.Parameters.Add(new SQLiteParameter("@log_status", System.Data.DbType.Int32) { Value = model.LogStatus });
                command.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }           
        }

        public void UploadAttendanceLogSuccess(long id, bool ok = true)
        {
            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                _connection.Open();
            }
            var command = new SQLiteCommand(_connection);
            command.CommandText = @"UPDATE attendance_logs SET sync=@sync,change_at=datetime('now') WHERE id=@id;";
            command.Parameters.Add(new SQLiteParameter("@id", System.Data.DbType.Int64) { Value = id });
            command.Parameters.Add(new SQLiteParameter("@sync", System.Data.DbType.Int32) { Value = ok ? 1 : 0 });
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

        public void Dispose()
        {
            if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
                _connection = null;
            }
        }   
    }
}
