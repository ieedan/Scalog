using Dapper;
using Scalog.Models;
using Scalog.Models.Database;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Xml.Linq;

namespace Scalog
{
    public class Logger : ILogger
    {
        private readonly bool _alwaysWriteToDatabase = false;
        private readonly string? _connectionString = null;
        private readonly string? _tableName = null;
        private readonly bool _isDev = true;

        /// <summary>
        /// Specifies the type of file to be logged to. The format will not change between .log and .txt files. Default is .log;
        /// </summary>
        public FileType FileExtension { get; set; } = FileType.log;

        /// <summary>
        /// When true the timestamps on the logs will use UTC DateTime.UtcNow instead of DateTime.Now
        /// </summary>
        public bool UseUtc { get; set; } = false;

        /// <summary>
        /// When true the logger will log to the Debug.WriteLine() instead of Console.WriteLine().
        /// The logger will still log to the file or database but this will be easier to access for debug mode.
        /// </summary>
        public bool LogToDebug { get; set; } = false;
        public bool WritingToDatabase
        {
            get { return _alwaysWriteToDatabase || (_connectionString != null && !_isDev); }
        }

        private readonly string? _path = null;
        public string Path
        {
            get
            {
                if (UseUtc)
                {
                    return $"{_path}{DateTime.UtcNow.Month}-{DateTime.UtcNow.Day}-{DateTime.UtcNow.Year}.{FileExtension.ToString()}";
                }
                else
                {
                    return $"{_path}{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Year}.{FileExtension.ToString()}";
                }
            }
            private set { }
        }

        /// <summary>
        /// Will default to the BasicLogger option
        /// </summary>
        public Logger()
        {
            _path = createPath();
        }

        /// <summary>
        /// Enables logging to the database
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableName"></param>
        public Logger(string connectionString, string tableName = "Logs")
        {
            _alwaysWriteToDatabase = true;
            _connectionString = connectionString;
            _tableName = tableName;
            generateSQL();
        }

        /// <summary>
        /// Enables logging to the database in production and to a local file in development
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="isDev"></param>
        /// <param name="tableName"></param>
        public Logger(string connectionString, bool isDev, string tableName = "Logs")
        {
            _isDev = isDev;
            _alwaysWriteToDatabase = false;
            _connectionString = connectionString;
            _tableName = tableName;
            _path = createPath();
            generateSQL();
        }

        /// <summary>
        /// Creates table and stored procedure
        /// </summary>
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

        public void LogError(object value, string type = "ERROR", [CallerMemberName] string? name = null)
        {
            // Will not log null values
            if (value == null)
                return;

            var log = new Log($"Called by: {name} - " + value.ToString(), type, UseUtc);
            writeLog(log);

            LoggedError?.Invoke(log);
        }

        public Task LogErrorAsync(object value, string type = "ERROR", [CallerMemberName] string? name = null)
        {
            // Will not log null values
            if (value == null)
                return Task.CompletedTask;

            var log = new Log($"Called by: {name} - " + value.ToString(), type, UseUtc);
            writeLog(log);

            LoggedError?.Invoke(log);

            return Task.CompletedTask;
        }

        public void LogInfo(object value, string type = "INFO")
        {
            // Will not log null values
            if (value == null)
                return;

            var log = new Log(value.ToString(), type, UseUtc);
            writeLog(log);

            LoggedInfo?.Invoke(log);
        }

        public Task LogInfoAsync(object value, string type = "INFO")
        {
            // Will not log null values
            if (value == null)
                return Task.CompletedTask;

            var log = new Log(value.ToString(), type, UseUtc);
            writeLog(log);

            LoggedInfo?.Invoke(log);

            return Task.CompletedTask;
        }

        private void writeLog(Log log)
        {
            if (WritingToDatabase)
            {
                writeToDatabase(log);
            }
            else
            {
                writeToLogFile(log);
            }

            if (LogToDebug)
            {
                Debug.WriteLine(log.ToString());
            }
            else
            {
                Console.WriteLine(log.ToString());
            }
        }

        private void writeToLogFile(Log log)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(Path, true))
                {
                    switch (FileExtension)
                    {
                        // Define other file extension formatting behaviors here
                        case (FileType.json):
                            writer.WriteLine(JsonSerializer.Serialize(log));
                            break;
                        default:
                            writer.WriteLine(log.ToString());
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to write to log file: " + ex.Message);
            }
        }

        public delegate void InfoLogged(Log log);

        /// <summary>
        /// Fires when info has been logged
        /// </summary>
        public event InfoLogged LoggedInfo;

        public delegate void ErrorLogged(Log log);

        /// <summary>
        /// Fires when an error has been logged
        /// </summary>
        public event ErrorLogged LoggedError;

        /// <summary>
        /// Uses stored procedure and Dapper to write the log to the database
        /// </summary>
        /// <param name="log"></param>
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
            catch (Exception ex)
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
        /// Gets a SQL string for creating a new table query can be found @ /SQL/Querys/CreateTable
        /// </summary>
        /// <returns></returns>
        private string getCreateTableSQL()
        {
            return $"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{_tableName}')\r\nBEGIN\r\n    CREATE TABLE [dbo].[{_tableName}](\r\n        [Id] [int] IDENTITY(1,1) NOT NULL,\r\n        [Date] datetime NOT NULL,\r\n        [Message] varchar(MAX) NOT NULL,\r\n        [Type] varchar(10) NOT NULL,\r\n        CONSTRAINT [PK_{_tableName}] PRIMARY KEY CLUSTERED \r\n        (\r\n            [Id] ASC\r\n        )WITH (\r\n            PAD_INDEX = OFF, \r\n            STATISTICS_NORECOMPUTE = OFF, \r\n            IGNORE_DUP_KEY = OFF, \r\n            ALLOW_ROW_LOCKS = ON, \r\n            ALLOW_PAGE_LOCKS = ON, \r\n            OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF\r\n        ) ON [PRIMARY]\r\n    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]\r\nEND";
        }

        /// <summary>
        /// Gets a SQL string for creating a procedure query can be found @ /SQL/Querys/CreateProcedure
        /// </summary>
        /// <returns></returns>
        private string getCreateProcedureSQL()
        {
            return $"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp{_tableName}_NewLog]') AND type in (N'P', N'PC'))\r\nBEGIN\r\n    EXECUTE('CREATE PROCEDURE [dbo].[sp{_tableName}_NewLog]\r\n    @Message varchar(MAX), @Date datetime, @Type varchar(10), @Id int = null\r\n    AS\r\n    BEGIN\r\n        INSERT INTO [{_tableName}] ([Message],[Date],[Type])\r\n        VALUES (@Message,@Date,@Type)\r\n    END')\r\nEND";
        }
    }
}
