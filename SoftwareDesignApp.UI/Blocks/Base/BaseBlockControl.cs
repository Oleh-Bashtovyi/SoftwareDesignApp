using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using SoftwareDesignApp.Core;

namespace SoftwareDesignApp.UI.Blocks.Base;

public class BaseBlockControl : UserControl, INotifyPropertyChanged
{
    private readonly List<BaseBlockControl> _incomingBlocks = [];
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

    public virtual string GetDisplayText()
    {
        return string.Empty;
    }

    public IReadOnlyCollection<BaseBlockControl> GetIncomingBlocks()
    {
        return _incomingBlocks;
    }

    public void AddIncomingBlock(BaseBlockControl incomingBlock)
    {
        _incomingBlocks.Add(incomingBlock);
    }

    public void RemoveIncomingBlock(BaseBlockControl incomingBlock)
    {
        _incomingBlocks.Remove(incomingBlock);
    }

    public virtual void RemoveConnection(BaseBlockControl block)
    {
    }

    public virtual Block ToCoreBlock(EndBlockControl endBlock)
    {
        throw new NotImplementedException("Can not create core block from base block control");
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