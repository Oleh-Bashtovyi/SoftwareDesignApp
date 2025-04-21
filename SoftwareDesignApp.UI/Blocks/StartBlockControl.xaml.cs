using SoftwareDesignApp.UI.Blocks.Base;

namespace SoftwareDesignApp.UI.Blocks;

public partial class StartBlockControl : OneNextBlockControl
{
    public string DiagramText => "Початок";

    public StartBlockControl(string blockId) : base(blockId)
    {
        InitializeComponent();
    }
}

