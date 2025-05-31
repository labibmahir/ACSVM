using Domain.Dto;
using System.ComponentModel.DataAnnotations;

namespace Api.BackGroundServices.ProccessContract
{
    public interface IProcess
    {
        string ProcessId { get; }

        [Range(0.0f, 100.0f)]
        float progress { get; set; }
        ProcessType ProcessType { get; }
        ProcessState ProcessState { get; set; }
        ProcessPriority ProcessPriority { get; }
        string ProcessName { get; set; }
        string ProcessDescription { get; set; }
        List<ErrorMessage> Errors { get; set; }

        ProcessDto ToProcessDto();
    }
    public enum ProcessType
    {
        AssignAccessLevelToPerson,
        ImportingPeopleFromDevice,
        RemovePerson,
        ImportingAcsEventFromDevice,
        ExportDataToDevice,
        DeviceAction
    }
    public enum ProcessState
    {
        Running,
        Waiting,
        Halting,
        Finished,
        Failed
    }
    public enum ProcessPriority
    {
        Normal,
        Urgent,
    }
}
