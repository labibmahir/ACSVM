namespace Api.BackGroundServices.ProccessContract
{
    public interface IProgressManager
    {
        Task AddProcess(IProcess process);
        Task RemoveProcess(string processId);
        Task<IProcess> GetProcess(string processId);
        Task<List<IProcess>> GetProcess(ProcessType processType);
        Task<List<IProcess>> GetAllProcess();
        Task UpdateProcessProgress(string processId, float newProgressValue);
        Task UpdateProcessState(string processId, ProcessState newProcessState);
        Task UpdateProcessName(string processId, string newName);
        Task UpdateProcessDescription(string processId, string newDescription);
    }
}
