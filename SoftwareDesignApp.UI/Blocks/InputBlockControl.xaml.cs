using SoftwareDesignApp.UI.Blocks.Base;

namespace SoftwareDesignApp.UI.Blocks;

public partial class InputBlockControl : OneNextBlockControl
{
    private string _var;

    public string Var
    {
        get => _var;
        set
        {
            SetField(ref _var, value);
            OnPropertyChanged(nameof(DiagramText));
        }
    }

    public string DiagramText => $"INPUT ({Var})";

    public InputBlockControl(string blockId, string var) : base(blockId)
    {
        _var = var;
        InitializeComponent();
    }
}

