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

        #region IdentifiedAssignCardRepository
        private IIdentifiedAssignCardRepository identifiedAssignCardRepository;
        public IIdentifiedAssignCardRepository IdentifiedAssignCardRepository
        {
            get
            {
                if (identifiedAssignCardRepository == null)
                    identifiedAssignCardRepository = new IdentifiedAssignCardRepository(context);

                return identifiedAssignCardRepository;
            }
        }
        #endregion IdentifiedAssignDeviceRepository

        #region PersonImageRepository
        private IPersonImageRepository personImageRepository;
        public IPersonImageRepository PersonImageRepository
        {
            get
            {
                if (personImageRepository == null)
                    personImageRepository = new PersonImageRepository(context);

                return personImageRepository;
            }
        }
        #endregion IdentifiedAssignDeviceRepository


        #region FingerPrintRepository
        private IFingerPrintRepository fingerPrintRepository;
        public IFingerPrintRepository FingerPrintRepository
        {
            get
            {
                if (fingerPrintRepository == null)
                    fingerPrintRepository = new FingerPrintRepository(context);

                return fingerPrintRepository;
            }
        }
        #endregion IdentifiedAssignDeviceRepository

        #region CardRepository
        private ICardRepository cardRepository;
        public ICardRepository CardRepository
        {
            get
            {
                if (cardRepository == null)
                    cardRepository = new CardRepository(context);

                return cardRepository;
            }
        }
        #endregion IdentifiedAssignDeviceRepository

        #region VisitorRepository
        private IVisitorRepository visitorRepository;
        public IVisitorRepository VisitorRepository
        {
            get
            {
                if (visitorRepository == null)
                    visitorRepository = new VisitorRepository(context);

                return visitorRepository;
            }
        }
        #endregion VisitorRepository

        #region AttendanceRepository
        private IAttendanceRepository attendanceRepository;
        public IAttendanceRepository AttendanceRepository
        {
            get
            {
                if (attendanceRepository == null)
                    attendanceRepository = new AttendanceRepository(context);

                return attendanceRepository;
            }
        }
        #endregion AttendanceRepository

        #region AppointmentRepository
        private IAppointmentRepository appointmentRepository;
        public IAppointmentRepository AppointmentRepository
        {
            get
            {
                if (appointmentRepository == null)
                    appointmentRepository = new AppointmentRepository(context);

                return appointmentRepository;
            }
        }
        #endregion AppointmentRepository

        #region AppointmentRepository
        private IIdentifiedAssignedAppointmentRepository identifiedAssignedAppointmentRepository;
        public IIdentifiedAssignedAppointmentRepository IdentifiedAssignedAppointmentRepository
        {
            get
            {
                if (identifiedAssignedAppointmentRepository == null)
                    identifiedAssignedAppointmentRepository = new IdentifiedAssignedAppointmentRepository(context);

                return identifiedAssignedAppointmentRepository;
            }
        }
        #endregion AppointmentRepository

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
