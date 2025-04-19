using SoftwareDesignApp.UI.Blocks.Base;

namespace SoftwareDesignApp.UI.Blocks;

public partial class PrintBlockControl : OneNextBlockControl
{
    private string _variable;

    public string Variable
    {
        get => _variable;
        set
        {
            SetField(ref _variable, value);
            OnPropertyChanged(nameof(DiagramText));
        }
    }

    public string DiagramText => $"Print({Variable})";

    public PrintBlockControl(string blockId, string variable) : base(blockId)
    {
        _variable = variable;
        InitializeComponent();
    }
}