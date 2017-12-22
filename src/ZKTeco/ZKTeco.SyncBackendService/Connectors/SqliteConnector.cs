using System;
using System.Data.SQLite;
using System.IO;
using Topshelf.Logging;
using ZKTeco.SyncBackendService.Bases;
using ZKTeco.SyncBackendService.Models;

namespace ZKTeco.SyncBackendService.Connectors
{
    internal class SqliteConnector : ServiceBase
    {
        const string SYNCDB = "syncdb.sqlite";

        private SQLiteConnectionStringBuilder _dbBuilder;

        private static string _dbPath;
        /// <summary>
        /// Database path
        /// </summary>
        private static string DbPath
        {
            get
            {
                if (_dbPath == null)
                {
                    _dbPath = Path.Combine(ZKTecoConfig.AppRootFolder, "data", SYNCDB);
                }
                return _dbPath;
            }
        }

        public SqliteConnector()
        {
            Logger = HostLogger.Get<SqliteConnector>();
            _dbBuilder = new SQLiteConnectionStringBuilder
            {
                DataSource = DbPath,
                Version = 3
            };
        }

        public void InsertAttendanceLog(AttendanceLog model)
        {
            var connection = new SQLiteConnection(_dbBuilder.ConnectionString);
            try
            {
                connection.Open();
            }
            catch (SQLiteException ex)
            {

            }
        }

        public static void InstallSyncDatabase()
        {
            if (!File.Exists(DbPath))
            {
                var dir = Path.GetDirectoryName(DbPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                SQLiteConnection.CreateFile(DbPath);
            }            
        } 
        
        public void CheckConnection()
        {            
            var connection = new SQLiteConnection(_dbBuilder.ConnectionString);
            try
            {
                connection.Open();
                CreateSchemaAndTables(connection);
            }
            catch (SQLiteException ex)
            {
                Logger.ErrorFormat("connection Open error: {@Error}", ex);
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

        private void CreateSchemaAndTables(SQLiteConnection connection)
        {
            var command = new SQLiteCommand(connection);
            CreateAttendanceLogTable(command);
            CreateAttendanceLogArchiveTable(command);
        }        

        private void CreateAttendanceLogTable(SQLiteCommand command)
        {
            var sql = @"CREATE TABLE IF NOT EXISTS attendance_logs(
                            id INT PRIMARY KEY AUTOINCREMENT,
                            machine_id INT NOT NULL,
                            enroll_number  TEXT NOT NULL,
                            log_date       TEXT NOT NULL,
                            create_at      TEXT  NOT NULL,
                            change_at      TEXT  NOT NULL,
                            sync           INT  NOT NULL
                        );";
            command.CommandText = sql;
            command.CommandType = System.Data.CommandType.Text;
            command.ExecuteNonQuery();
        }
        
        private void CreateAttendanceLogArchiveTable(SQLiteCommand command)
        {
            var sql = @"CREATE TABLE IF NOT EXISTS attendance_logs_archive(
                            id INT PRIMARY KEY AUTOINCREMENT,
                            refer_no    INT NOT NULL,
                            machine_id INT NOT NULL,
                            enroll_number  TEXT NOT NULL,
                            log_date       TEXT NOT NULL,
                            create_at      TEXT  NOT NULL
                        );";
            command.CommandText = sql;
            command.CommandType = System.Data.CommandType.Text;
            command.ExecuteNonQuery();
        }
        
    }
}
