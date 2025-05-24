using Domain.Dto;
using Domain.Entities;
using Infrastructure.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helper
{
    public class CustomConnectionStringService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<ClientDBDetail> logger;

        public CustomConnectionStringService(IUnitOfWork unitOfWork, ILogger<ClientDBDetail> logger)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<string> GetConnectionStringAsync()
        {
            var config = await unitOfWork.ClientDBDetailRepository.FirstOrDefaultAsync(c => c.IsConnectionActive == true);

            if (config == null)
            {
                throw new Exception("No database configuration found for the specified client.");
            }

            return BuildConnectionString(config);
        }

        public string BuildConnectionString(ClientDBDetail config)
        {
            return config.DatabaseType.ToString() switch
            {
                "SQLServer" => $"Server={config.ServerName};Database={config.DatabaseName};User Id={config.UserId};Password={config.Password};TrustServerCertificate=True;",
                "CustomSQLServer" => $"Server={config.ServerName};Database={config.DatabaseName};User Id={config.UserId};Password={config.Password};TrustServerCertificate=True;",
                "MySQL" => $"Server={config.ServerName};Database={config.DatabaseName};User Id={config.UserId};Password={config.Password}",
                "PostgreSQL" => $"Host={config.ServerName};Port={config.Port};Database={config.DatabaseName};Username={config.UserId};Password={config.Password}",
                "Oracle" => $"Data Source={config.ServerName + ':' + config.Port + '/' + config.ServiceName};DBA Privilege={config.Role};User Id={config.UserId};Password={config.Password}",
                _ => $"Unsupported database type."
            };
        }

        public string BuildConnectionStringFromUI(ClientDBDetailDto config)
        {
            return config.DatabaseType.ToString() switch
            {
                "SQLServer" => $"Server={config.ServerName};Database={config.DatabaseName};User Id={config.UserId};Password={config.Password};TrustServerCertificate=True;",
                "CustomSQLServer" => $"Server={config.ServerName};Database={config.DatabaseName};User Id={config.UserId};Password={config.Password};TrustServerCertificate=True;",
                "MySQL" => $"Server={config.ServerName};Database={config.DatabaseName};User Id={config.UserId};Password={config.Password}",
                "PostgreSQL" => $"Host={config.ServerName};Port={config.Port};Database={config.DatabaseName};Username={config.UserId};Password={config.Password}",
                "Oracle" => $"Data Source={config.ServerName + ':' + config.Port + '/' + config.ServiceName};DBA Privilege={config.Role};User Id={config.UserId};Password={config.Password}",
                _ => $"Unsupported database type."
            };
        }
    }
}
