using LarpakeServer.Models.EventModels;

namespace LarpakeServer.Services;

public class CompletionMessageService 
{
    public void SendAttendanceCompletedMessage(AttendedCreated metadata)
    {
        TaskReceived?.Invoke(this, metadata);
    }

    public event EventHandler<AttendedCreated>? TaskReceived;
}
