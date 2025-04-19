using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace SoftwareDesignApp.UI.Blocks.Base;

public class BaseBlockControl : UserControl, INotifyPropertyChanged
{
    private bool _isSelected;

    public event PropertyChangedEventHandler? PropertyChanged;
    public string BlockId { get; init; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }

    public BaseBlockControl(string blockId)
    {
        BlockId = blockId;
        DataContext = this;
    }

    public void MarkAsSelected()
    {
        IsSelected = true;
    }

    public void UnMarkAsSelected()
    {
        IsSelected = false;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}