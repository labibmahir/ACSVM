using Infrastructure.Contracts;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    /// <summary>
    /// Implementation of IUnitOfWork.
    /// </summary>
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        protected readonly DataContext context;
        protected readonly IConfiguration configuration;
        private bool _disposed;

        public UnitOfWork(DataContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await context.SaveChangesAsync();
        }

        //USER ACCOUNT
        #region UserAccountRepository
        private IUserAccountRepository userAccountRepository;
        public IUserAccountRepository UserAccountRepository
        {
            get
            {
                if (userAccountRepository == null)
                    userAccountRepository = new UserAccountRepository(context);

                return userAccountRepository;
            }
        }
        #endregion UserAccountRepository

        #region DeviceRepository
        private IDeviceRepository deviceRepository;
        public IDeviceRepository DeviceRepository
        {
            get
            {
                if (deviceRepository == null)
                    deviceRepository = new DeviceRepository(context);

                return deviceRepository;
            }
        }
        #endregion DeviceRepository

        #region AccessLevelRepository
        private IAccessLevelRepository accessLevelRepository;
        public IAccessLevelRepository AccessLevelRepository
        {
            get
            {
                if (accessLevelRepository == null)
                    accessLevelRepository = new AccessLevelRepository(context);

                return accessLevelRepository;
            }
        }
        #endregion AccessLevelRepository

        #region IdentifiedAssignDeviceRepository
        private IIdentifiedAssignDeviceRepository identifiedAssignDeviceRepository;
        public IIdentifiedAssignDeviceRepository IdentifiedAssignDeviceRepository
        {
            get
            {
                if (identifiedAssignDeviceRepository == null)
                    identifiedAssignDeviceRepository = new IdentifiedAssignDeviceRepository(context);

                return identifiedAssignDeviceRepository;
            }
        }
        #endregion IdentifiedAssignDeviceRepository

        #region PersonRepository
        private IPersonRepository personRepository;
        public IPersonRepository PersonRepository
        {
            get
            {
                if (personRepository == null)
                    personRepository = new PersonRepository(context);

                return personRepository;
            }
        }
        #endregion IdentifiedAssignDeviceRepository

        #region Entity LifeSpan
        public IDbContextTransaction BeginTransaction()
        {
            return context.Database.BeginTransaction();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await context.Database.BeginTransactionAsync();
        }

        protected void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }

            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
