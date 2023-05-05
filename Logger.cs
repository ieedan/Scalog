using Dapper;
using Scalog.Models;
using Scalog.Models.Database;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Scalog
{
    public class Logger : ILogger
    {
        private readonly bool _alwaysWriteToDatabase = false;
        private readonly string? _connectionString = null;
        private readonly string? _tableName = null;
        private static bool isDev
        {
            get
            {
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                return string.IsNullOrEmpty(environment) || environment.ToLower() == "development";
            }
        }

        private readonly string? _path = null;
        public string Path
        {
            get
            {
                return $"{_path}{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Year}.log";
            }
        }

        /// <summary>
        /// Will default to the BasicLogger option
        /// </summary>
        public Logger()
        {
            _path = createPath();
        }

        /// <summary>
        /// Enables logging to a SQL database
        /// </summary>
        /// <param name="connectionString"></param> If you leave the table name blank this needs privelages to CREATE
        /// <param name="tableName"></param> Leave this blank for Scalog to create a table for you
        /// <param name="alwaysWriteToDatabase"></param> Set this to false if you want to write to a database only when in production
        public Logger(string connectionString, string tableName = "Logs", bool alwaysWriteToDatabase = true)
        {
            if (!alwaysWriteToDatabase) _path = createPath();

            _alwaysWriteToDatabase = alwaysWriteToDatabase;
            _connectionString = connectionString;
            _tableName = tableName;
            generateSQL();
        }

        private async void generateSQL()
        {
            await createTable();
            await createStoredProcedure();
        }

        /// <summary>
        /// Creates the path to the log
        /// </summary>
        /// <returns></returns>
        private string createPath()
        {
            string fullPath = AppDomain.CurrentDomain.BaseDirectory + "Logs";
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            return $"{fullPath}\\";
        }

        public void LogError(string message)
        {
            var log = new Log(message, "ERROR");
            writeLog(log);
        }

        public Task LogErrorAsync(string message)
        {
            var log = new Log(message, "ERROR");
            writeLog(log);

            return Task.CompletedTask;
        }

        public void LogInfo(string message)
        {
            var log = new Log(message, "INFO");
            writeLog(log);
        }

        public Task LogInfoAsync(string message)
        {
            var log = new Log(message, "INFO");
            writeLog(log);

            return Task.CompletedTask;
        }

        private void writeLog(Log log)
        {
            if (_alwaysWriteToDatabase || _connectionString != null && !isDev)
            {
                writeToDatabase(log);
            }
            else
            {
                writeToLogFile(log);
            }
        }

        private void writeToLogFile(Log log)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(Path, true))
                {
                    writer.WriteLine(log.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to write to log file: " + ex.Message);
            }
        }

        private void writeToDatabase(Log log)
        {
            string procedureName = $"sp{_tableName}_NewLog";
            try
            {
                using (IDbConnection cnn = new SqlConnection(_connectionString))
                {
                    CommandType? commandType = CommandType.StoredProcedure;
                    cnn.Query<Log>(procedureName, log, null, buffered: true, null, commandType);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private Task createTable()
        {
            string query = getCreateTableSQL();
            using (IDbConnection cnn = new SqlConnection(_connectionString))
            {
                cnn.Query(query);
            }
            return Task.CompletedTask;
        }

        private Task createStoredProcedure()
        {
            string query = getCreateProcedureSQL();
            using (IDbConnection cnn = new SqlConnection(_connectionString))
            {
                cnn.Query(query);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets a SQL string for creating a new table
        /// </summary>
        /// <returns></returns>
        private string getCreateTableSQL()
        {
            return $"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{_tableName}')\r\nBEGIN\r\n    CREATE TABLE [dbo].[{_tableName}](\r\n        [Id] [int] IDENTITY(1,1) NOT NULL,\r\n        [Date] datetime NOT NULL,\r\n        [Message] varchar(MAX) NOT NULL,\r\n        [Type] varchar(10) NOT NULL,\r\n        CONSTRAINT [PK_{_tableName}] PRIMARY KEY CLUSTERED \r\n        (\r\n            [Id] ASC\r\n        )WITH (\r\n            PAD_INDEX = OFF, \r\n            STATISTICS_NORECOMPUTE = OFF, \r\n            IGNORE_DUP_KEY = OFF, \r\n            ALLOW_ROW_LOCKS = ON, \r\n            ALLOW_PAGE_LOCKS = ON, \r\n            OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF\r\n        ) ON [PRIMARY]\r\n    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]\r\nEND";
        }
        /// <summary>
        /// Gets a SQL string for creating a procedure
        /// </summary>
        /// <returns></returns>
        private string getCreateProcedureSQL()
        {
            return $"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp{_tableName}_NewLog]') AND type in (N'P', N'PC'))\r\nBEGIN\r\n    EXECUTE('CREATE PROCEDURE [dbo].[sp{_tableName}_NewLog]\r\n    @Message varchar(MAX), @Date datetime, @Type varchar(10), @Id int = null\r\n    AS\r\n    BEGIN\r\n        INSERT INTO [{_tableName}] ([Message],[Date],[Type])\r\n        VALUES (@Message,@Date,@Type)\r\n    END')\r\nEND";
        }
    }
}
