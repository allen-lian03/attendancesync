using System;
using System.Data.SQLite;
using System.IO;
using Topshelf.Logging;

namespace ZKTeco.SyncBackendService.Connectors
{
    internal class SqliteConnector
    {
        const string SYNCDB = "syncdb.sqlite";

        private string _dbPath;

        private LogWriter _logger;

        public SqliteConnector()
        {
            _logger = HostLogger.Get<SqliteConnector>();
            _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", SYNCDB);
        }

        public void Open()
        {

        }
        
        public void InstallSyncDatabase()
        {
            if (!File.Exists(_dbPath))
            {
                SQLiteConnection.CreateFile(_dbPath);
            }

            var builder = new SQLiteConnectionStringBuilder
            {
                DataSource = _dbPath,
                Version = 3
            };  
            var connection = new SQLiteConnection(builder.ConnectionString);
            try
            {
                connection.Open();
                CreateSchemaAndTables(connection);
            }
            catch (SQLiteException ex)
            {
                _logger.ErrorFormat("connection Open error: {@Error}", ex);
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
