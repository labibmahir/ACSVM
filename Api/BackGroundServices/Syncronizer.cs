using Api.BackGroundServices.ProccessContract;
using Api.BackGroundServices.ProcessImplimentations;
using Infrastructure.Contracts;
using SurveillanceDevice.Integration.HIKVision;

namespace Api.BackGroundServices
{
    public class Syncronizer : BackgroundService
    {
        private readonly ILogger<Syncronizer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IProgressManager _progressManager;

        public Syncronizer(ILogger<Syncronizer> logger, IServiceProvider serviceProvider, IProgressManager progressManager)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _progressManager = progressManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Syncronizer running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken); // 3600000

                var process = (await _progressManager.GetAllProcess())
                    .Where(x => x.ProcessState != ProcessState.Finished
                    && x.ProcessState != ProcessState.Failed
                    && x.ProcessState != ProcessState.Running).FirstOrDefault();
                if (process is ImportPeopleFromDeviceProcess importProcess)
                {
                    _ = ImportPeopleFromDevice(importProcess);
                }
                if (process is ExportPeopleToDeviceProcess exportProcess)
                {
                    _ = ExportPeopleToDevice(exportProcess);
                }

                if (process is DeviceActionProcess deviceActionProcess)
                {
                    _ = DeviceActions(deviceActionProcess);
                }

            }
        }

        private async Task ImportPeopleFromDevice(ImportPeopleFromDeviceProcess process)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<ImportPeopleFromDeviceProcess>>();
                    var visionMachineService = scope.ServiceProvider.GetRequiredService<IHikVisionMachineService>();

                    await process.Execute(logger, unitOfWork, visionMachineService);
                    await _progressManager.RemoveProcess(process.ProcessId);
                }
            }
            catch
            {

            }
        }

        private async Task ExportPeopleToDevice(ExportPeopleToDeviceProcess process)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<ExportPeopleToDeviceProcess>>();
                    var visionMachineService = scope.ServiceProvider.GetRequiredService<IHikVisionMachineService>();

                    await process.Execute(logger, unitOfWork, visionMachineService);
                    await _progressManager.RemoveProcess(process.ProcessId);
                }
            }
            catch
            {

            }
        }
        private async Task DeviceActions(DeviceActionProcess process)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<DeviceActionProcess>>();
                    var visionMachineService = scope.ServiceProvider.GetRequiredService<IHikVisionMachineService>();

                    await process.Execute(logger, unitOfWork, visionMachineService);
                    await _progressManager.RemoveProcess(process.ProcessId);
                }
            }
            catch
            {

            }
        }
    }
}