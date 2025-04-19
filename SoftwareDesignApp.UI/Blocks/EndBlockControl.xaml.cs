using SoftwareDesignApp.UI.Blocks.Base;

namespace SoftwareDesignApp.UI.Blocks;

public partial class EndBlockControl : BaseBlockControl
{
    public string DiagramText => "Кінець";

    public EndBlockControl(string blockId) : base(blockId)
    {
        InitializeComponent();
    }
}

