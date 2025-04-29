using SoftwareDesignApp.Core;
using SoftwareDesignApp.UI.Blocks.Base;

namespace SoftwareDesignApp.UI.Blocks;

public partial class ConstantBlockControl : OneNextBlockControl
{
    private string _var;
    private int _value;

    public string Var
    {
        get => _var;
        set { SetField(ref _var, value); OnPropertyChanged(nameof(DiagramText)); }
    }

    public int Value
    {
        get => _value;
        set { SetField(ref _value, value); OnPropertyChanged(nameof(DiagramText)); }
    }

    public string DiagramText => $"{Var} = {Value}";

    public ConstantBlockControl(string blockId, string var, int value) : base(blockId)
    {
        _var = var;
        _value = value;
        InitializeComponent();
    }

    public override Block ToCoreBlock()
    {
        return new ConstantBlock(BlockId, Var, Value, NextBlock?.BlockId);
    }

    public override string GetDisplayText()
    {
        return $"Id:{BlockId}, {DiagramText}";
    }
}
