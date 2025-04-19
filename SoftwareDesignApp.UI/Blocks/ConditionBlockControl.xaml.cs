using SoftwareDesignApp.UI.Blocks.Base;

namespace SoftwareDesignApp.UI.Blocks;

public partial class ConditionBlockControl : BaseBlockControl
{
    private string _var;
    private int _value;
    private string _condition;
    private BaseBlockControl? _trueConditionNextBlock;
    private BaseBlockControl? _falseConditionNextBlock;

    public BaseBlockControl? TrueConditionNextBlock => _trueConditionNextBlock;

    public BaseBlockControl? FalseConditionNextBlock => _falseConditionNextBlock;

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

    public string Condition
    {
        get => _condition;
        set { SetField(ref _condition, value); OnPropertyChanged(nameof(DiagramText)); }
    }

    public string DiagramText => $"{Var} {Condition} {Value}";

    public ConditionBlockControl(string blockId, string var, int value, string condition) : base(blockId)
    {
        _var = var;
        _value = value;
        _condition = condition;
        InitializeComponent();
    }

    public void SetNextBlocks(BaseBlockControl trueConditionNextBlock, BaseBlockControl falseConditionNextBlock)
    {
        _trueConditionNextBlock = trueConditionNextBlock;
        _falseConditionNextBlock = falseConditionNextBlock;
    }
}
