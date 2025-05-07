using SoftwareDesignApp.Core;
using SoftwareDesignApp.UI.Blocks.Base;

namespace SoftwareDesignApp.UI.Blocks;

public partial class DelayBlockControl : OneNextBlockControl
{
    private int _delayMs;

    public int DelayMs
    {
        get => _delayMs;
        set { SetField(ref _delayMs, value); OnPropertyChanged(nameof(DiagramText)); }
    }

    public string DiagramText => $"Delay {DelayMs} ms";

    public DelayBlockControl(string blockId, int delayMs) : base(blockId)
    {
        _delayMs = delayMs;
        InitializeComponent();
    }

    public override Block ToCoreBlock(EndBlockControl endBlock)
    {
        return new DelayBlock(BlockId, DelayMs, NextBlock?.BlockId ?? endBlock.BlockId);
    }

    public override string GetDisplayText()
    {
        return $"Id:{BlockId}, {DiagramText}";
    }
}