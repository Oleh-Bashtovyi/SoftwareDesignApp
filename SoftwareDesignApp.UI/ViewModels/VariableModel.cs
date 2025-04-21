namespace SoftwareDesignApp.UI.ViewModels;

public class VariableModel(string name, int value = 0) : BaseViewModel
{
    private int _value = value;

    public string Name { get; } = name;

    public int Value
    {
        get => _value;
        set
        {
            SetField(ref _value, value);
            OnPropertyChanged(nameof(DisplayText));
        }
    }

    public string DisplayText => $"{Name} = {Value}";
}