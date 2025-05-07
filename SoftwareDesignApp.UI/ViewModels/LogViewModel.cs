namespace SoftwareDesignApp.UI.ViewModels;

public class LogViewModel(string message, DateTime timestamp) : BaseViewModel
{
    private string _message = message;
    private DateTime _timestamp = timestamp;

    public string Message
    {
        get => _message;
        set => SetField(ref _message, value);
    }

    public DateTime Timestamp
    {
        get => _timestamp;
        set => SetField(ref _timestamp, value);
    }
}