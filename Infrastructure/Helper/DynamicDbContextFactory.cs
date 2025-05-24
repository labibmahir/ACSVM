using Domain.Dto;
using Microsoft.Data.SqlClient;
using MySqlConnector;
using Newtonsoft.Json.Linq;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helper
{
    public class DynamicDbContextFactory
    {
        private readonly CustomConnectionStringService connectionStringService;

        public DynamicDbContextFactory(CustomConnectionStringService connectionStringService)
        {
            this.connectionStringService = connectionStringService;
        }

        public async Task<IDbConnection> CreateDbContextAsync(string dbType)
        {

            string connectionString = "";

            if (dbType == "Default")
            {
                connectionString = GetSQLConnectionString();
            }
            else
            {
                connectionString = await connectionStringService.GetConnectionStringAsync();
            }

            //var optionsBuilder = new DbContextOptionsBuilder<ClientDBContext>();

            IDbConnection connection = dbType.ToString() switch
            {
                "CustomSQLServer" => new SqlConnection(connectionString),
                "MySQL" => new MySqlConnection(connectionString),
                "PostgreSQL" => new NpgsqlConnection(connectionString),
                "Oracle" => new OracleConnection(connectionString),
                _ => new SqlConnection(connectionString)
                //_ => throw new Exception("Unsupported database type.")
            };

            return connection;
        }

        public IDbConnection CreateRemoteDbContext()
        {
            string connectionString = GetRemoteSQLConnectionString();

            IDbConnection connection = new SqlConnection(connectionString);

            return connection;
        }

        public IDbConnection CheckDbContextAsync(ClientDBDetailDto config)
        {
            string connectionString = "";

            if (config != null)
            {
                connectionString = connectionStringService.BuildConnectionStringFromUI(config);
            }

            //var optionsBuilder = new DbContextOptionsBuilder<ClientDBContext>();

            IDbConnection connection = config.DatabaseType.ToString() switch
            {
                "CustomSQLServer" => new SqlConnection(connectionString),
                "MySQL" => new MySqlConnection(connectionString),
                "PostgreSQL" => new NpgsqlConnection(connectionString),
                "Oracle" => new OracleConnection(connectionString),
                _ => new SqlConnection(connectionString)
                //_ => throw new Exception("Unsupported database type.")
            };

            return connection;
        }

        public string GetSQLConnectionString()
        {
            var configFilePath = "appsettings.json";

            string connectionString = string.Empty;

            if (!File.Exists(configFilePath))
            {
                Console.WriteLine($"Configuration file {configFilePath} not found.");
            }

            var config = JObject.Parse(File.ReadAllText(configFilePath));

            // Retrieve the DatabasePath from the config
            string? databasePath = config["ConnectionStrings"]?["DefaultConnection"]?.ToString();

            if (string.IsNullOrEmpty(databasePath))
            {
                Console.WriteLine("No valid database path found in config.json.");
            }
            else
            {
                // Create the connection string dynamically
                connectionString = $"{databasePath};";
            }

            return connectionString;
        }

        public string GetRemoteSQLConnectionString()
        {
            var configFilePath = "appsettings.json";

            string connectionString = string.Empty;

            if (!File.Exists(configFilePath))
            {
                Console.WriteLine($"Configuration file {configFilePath} not found.");
            }

            var config = JObject.Parse(File.ReadAllText(configFilePath));

            // Retrieve the DatabasePath from the config
            string? databasePath = config["ConnectionStrings"]?["RemoteConn"]?.ToString();

            if (string.IsNullOrEmpty(databasePath))
            {
                Console.WriteLine("No valid database path found in config.json.");
            }
            else
            {
                // Create the connection string dynamically
                connectionString = $"{databasePath};";
            }

            return connectionString;
        }
    }
}
