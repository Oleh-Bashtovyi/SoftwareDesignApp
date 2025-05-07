using SoftwareDesignApp.Core;
using SoftwareDesignApp.UI.Blocks.Base;

namespace SoftwareDesignApp.UI.Blocks;

public partial class MathOperationBlockControl : OneNextBlockControl
{
    private string _targetVariable;
    private string _operation;
    private string _operationVariable;

    public string TargetVariable
    {
        get => _targetVariable;
        set { SetField(ref _targetVariable, value); OnPropertyChanged(nameof(DiagramText)); }
    }

    public string Operation
    {
        get => _operation;
        set { SetField(ref _operation, value); OnPropertyChanged(nameof(DiagramText)); }
    }

    public string OperationVariable
    {
        get => _operationVariable;
        set { SetField(ref _operationVariable, value); OnPropertyChanged(nameof(DiagramText)); }
    }

    public string DiagramText => $"{TargetVariable} = {TargetVariable} {Operation} {OperationVariable}";

    public MathOperationBlockControl(
        string blockId, 
        string targetVariable,
        string operation, 
        string operationVariable) : base(blockId)
    {
        _targetVariable = targetVariable;
        _operation = operation;
        _operationVariable = operationVariable;
        InitializeComponent();
    }

    public override Block ToCoreBlock(EndBlockControl endBlock)
    {
        return new MathOperationBlock(BlockId, TargetVariable, Operation, OperationVariable, NextBlock?.BlockId ?? endBlock.BlockId);
    }

    public override string GetDisplayText()
    {
        return $"Id:{BlockId}, {DiagramText}";
    }
}