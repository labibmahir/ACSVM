using Api.BackGroundServices.ProccessContract;
using SurveillanceDevice.Integration.HIKVision;

namespace Api.BackGroundServices.ProcessImplimentations
{
    public class ProgressManager : IProgressManager
    {
        //private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<IProgressManager> logger;
        private readonly IHikVisionMachineService _visionMachineService;

        private readonly object ProcessesLock = new object();
        private List<IProcess> Processes { get; set; } = new List<IProcess>();
        public ProgressManager(
            ILogger<IProgressManager> logger
            , IHikVisionMachineService visionMachineService)
        {
            this.logger = logger;
            _visionMachineService = visionMachineService;
        }

        public async Task AddProcess(IProcess process)
        {
            lock (ProcessesLock)
            {

                Processes.Add(process);
            }
        }

        public async Task RemoveProcess(string processId)
        {
            lock (ProcessesLock)
            {
                var process = Processes.FirstOrDefault(x => x.ProcessId == processId);
                if (process!= null)
                {
                    Processes.Remove(process);
                }
            }
        }

        public Task<IProcess?> GetProcess(string processId)
        {
            IProcess? process;
            lock (ProcessesLock)
            {
                process = Processes.FirstOrDefault(x => x.ProcessId == processId);
            }
            return Task.FromResult(process);
        }

        public Task<List<IProcess>> GetProcess(ProcessType processType)
        {
            List<IProcess> process = new List<IProcess>();
            lock (ProcessesLock)
            {
                process = Processes.Where(x => x.ProcessType == processType).ToList();
            }
            return Task.FromResult(process);
        }
        public Task<List<IProcess>> GetAllProcess()
        {
            List<IProcess> process = new List<IProcess>();
            lock (ProcessesLock)
            {
                process = Processes.ToList();
            }
            return Task.FromResult(process);
        }

        public async Task UpdateProcessProgress(string processId, float newProgressValue)
        {
            IProcess? process;
            lock (ProcessesLock)
            {
                process = Processes.FirstOrDefault(x => x.ProcessId == processId);
                if (process != null)
                {
                    process.progress = newProgressValue;
                }
            }
        }

        public async Task UpdateProcessState(string processId, ProcessState newProcessState)
        {
            IProcess? process;
            lock (ProcessesLock)
            {
                process = Processes.FirstOrDefault(x => x.ProcessId == processId);
                if (process != null)
                {
                    process.ProcessState = newProcessState;
                }
            }
        }

        public async Task UpdateProcessName(string processId, string newName)
        {
            IProcess? process;
            lock (ProcessesLock)
            {
                process = Processes.FirstOrDefault(x => x.ProcessId == processId);
                if (process != null)
                {
                    process.ProcessName = newName;
                }
            }
        }

        public async Task UpdateProcessDescription(string processId, string newDescription)
        {
            IProcess? process;
            lock (ProcessesLock)
            {
                process = Processes.FirstOrDefault(x => x.ProcessId == processId);
                if (process != null)
                {
                    process.ProcessDescription = newDescription;
                }
            }
        }
    }
}