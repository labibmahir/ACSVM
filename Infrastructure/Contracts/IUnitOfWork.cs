using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Contracts
{
    /// <summary>
    /// IUnitOfWork.
    /// </summary>
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();

        IDbContextTransaction BeginTransaction();

        Task<IDbContextTransaction> BeginTransactionAsync();


        //USER MODULE:
        IUserAccountRepository UserAccountRepository { get; }
        IDeviceRepository DeviceRepository { get; }
        IAccessLevelRepository AccessLevelRepository { get; }
        IIdentifiedAssignDeviceRepository IdentifiedAssignDeviceRepository { get; }
        IPersonRepository PersonRepository { get; }
        IIdentifiedAssignCardRepository IdentifiedAssignCardRepository { get; }
        IPersonImageRepository PersonImageRepository { get; }
        IFingerPrintRepository FingerPrintRepository { get; }
        ICardRepository CardRepository { get; }
        IVisitorRepository VisitorRepository { get; }
        IAttendanceRepository AttendanceRepository { get; }
        IAppointmentRepository AppointmentRepository { get; }
        IIdentifiedAssignedAppointmentRepository IdentifiedAssignedAppointmentRepository { get; }
        IClientDBDetailRepository ClientDBDetailRepository { get; }
        IClientFieldMappingRepository ClientFieldMappingRepository { get; }
        IDeviceSynchronizerRepository DeviceSynchronizerRepository { get; }
        IIdentifiedSyncDeviceRepository IdentifiedSyncDeviceRepository { get; }
        IDeviceLogRepository DeviceLogRepository { get; }
        IVisitorLogRepository VisitorLogRepository { get; }
    }
}
