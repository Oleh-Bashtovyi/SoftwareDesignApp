using System.Windows.Media;
using SoftwareDesignApp.UI.Blocks.Base;

namespace SoftwareDesignApp.GUI.BlocksNew;

public partial class AssignmentBlockControl : OneNextBlockControl
{
    private string _var1;
    private string _var2;

    public string Var1
    {
        get => _var1;
        set { SetField(ref _var1, value); OnPropertyChanged(nameof(DiagramText)); }
    }

    public string Var2
    {
        get => _var2;
        set { SetField(ref _var2, value); OnPropertyChanged(nameof(DiagramText)); }
    }

    public string DiagramText => $"{Var1} = {Var2}";

    public AssignmentBlockControl(string blockId, string var1, string var2) : base(blockId)
    {
        _var1 = var1;
        _var2 = var2;
        InitializeComponent();
    }
}

