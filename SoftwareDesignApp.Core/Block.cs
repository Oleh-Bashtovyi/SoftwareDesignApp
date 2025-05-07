namespace SoftwareDesignApp.Core;

public abstract class Block
{
    public string BlockId { get; }

    protected Block(string blockId)
    {
        BlockId = blockId ?? throw new ArgumentNullException(nameof(blockId));
    }

    public abstract string GenerateCode(int indentationLevel);

    protected string GetIndentation(int level)
    {
        return new string(' ', level * 4);
    }

    protected static string AssignTrimmed(string? value, [System.Runtime.CompilerServices.CallerArgumentExpression("value")] string name = "")
        => value?.Trim() ?? throw new ArgumentNullException(name);

    protected static string Assign(string? value, [System.Runtime.CompilerServices.CallerArgumentExpression("value")] string name = "")
        => value ?? throw new ArgumentNullException(name);
}

public class StartBlock : Block
{
    public string NextBlockId { get; }

    public StartBlock(string blockId, string nextBlockId) : base(blockId)
    {
        NextBlockId = nextBlockId ?? throw new ArgumentNullException(nameof(nextBlockId));
    }

    public override string GenerateCode(int indentationLevel)
    {
        var indent = GetIndentation(indentationLevel);
        return $"{indent}{BlockId}:\n" +
               $"{indent}// Start execution\n" +
               $"{indent}goto {NextBlockId};";
    }
}
public class EndBlock : Block
{
    public EndBlock(string blockId) : base(blockId)
    {
    }

    public override string GenerateCode(int indentationLevel)
    {
        var indent = GetIndentation(indentationLevel);
        return $"{indent}{BlockId}:\n" +
               $"{indent}// End execution\n" +
               $"{indent}return;";
    }
}

public class AssignmentBlock : Block
{
    public string TargetVariable { get; }
    public string SourceVariable { get; }
    public string NextBlockId { get; }

    public AssignmentBlock(string blockId, string targetVariable, string sourceVariable, string nextBlockId)
        : base(blockId)
    {
        TargetVariable = AssignTrimmed(targetVariable);
        SourceVariable = AssignTrimmed(sourceVariable);
        NextBlockId = Assign(nextBlockId);
    }

    public override string GenerateCode(int indentationLevel)
    {
        var indent = GetIndentation(indentationLevel);
        return $"{indent}{BlockId}:\n" +
               $"{indent}{TargetVariable} = {SourceVariable};\n" +
               $"{indent}goto {NextBlockId};";
    }
}

public class ConstantBlock : Block
{
    public string TargetVariable { get; }
    public int Value { get; }
    public string NextBlockId { get; }

    public ConstantBlock(string blockId, string targetVariable, int value, string nextBlockId)
        : base(blockId)
    {
        TargetVariable = AssignTrimmed(targetVariable);
        Value = value;
        NextBlockId = Assign(nextBlockId);
    }

    public override string GenerateCode(int indentationLevel)
    {
        var indent = GetIndentation(indentationLevel);
        return $"{indent}{BlockId}:\n" +
               $"{indent}{TargetVariable} = {Value};\n" +
               $"{indent}goto {NextBlockId};";
    }
}

public class InputBlock : Block
{
    public string TargetVariable { get; }
    public string NextBlockId { get; }

    public InputBlock(string blockId, string targetVariable, string nextBlockId)
        : base(blockId)
    {
        TargetVariable = AssignTrimmed(targetVariable);
        NextBlockId = Assign(nextBlockId);
    }

    public override string GenerateCode(int indentationLevel)
    {
        var indent = GetIndentation(indentationLevel);
        return $"{indent}{BlockId}:\n" +
               $"{indent}Console.Write(\"Enter value for {TargetVariable}: \");\n" +
               $"{indent}{TargetVariable} = Convert.ToInt32(Console.ReadLine());\n" +
               $"{indent}goto {NextBlockId};";
    }
}

public class PrintBlock : Block
{
    public string SourceVariable { get; }
    public string NextBlockId { get; }

    public PrintBlock(string blockId, string sourceVariable, string nextBlockId)
        : base(blockId)
    {
        SourceVariable = AssignTrimmed(sourceVariable);
        NextBlockId = Assign(nextBlockId);
    }

    public override string GenerateCode(int indentationLevel)
    {
        var indent = GetIndentation(indentationLevel);
        return $"{indent}{BlockId}:\n" +
               $"{indent}Console.WriteLine(\"{SourceVariable} = \" + {SourceVariable});\n" +
               $"{indent}goto {NextBlockId};";
    }
}

public class ConditionBlock : Block
{
    public string Variable { get; }
    public string Operator { get; }
    public string Value { get; }
    public string YesBranchBlockId { get; }
    public string NoBranchBlockId { get; }

    public ConditionBlock(string blockId, string variable, string operatorSymbol,
                         string value, string yesBranchBlockId, string noBranchBlockId)
        : base(blockId)
    {
        Variable = AssignTrimmed(variable);
        Operator = AssignTrimmed(operatorSymbol);
        Value = AssignTrimmed(value);
        YesBranchBlockId = Assign(yesBranchBlockId);
        NoBranchBlockId = Assign(noBranchBlockId);
    }

    public override string GenerateCode(int indentationLevel)
    {
        var indent = GetIndentation(indentationLevel);
        return $"{indent}{BlockId}:\n" +
               $"{indent}if ({Variable} {Operator} {Value})\n" +
               $"{indent}    goto {YesBranchBlockId};\n" +
               $"{indent}else\n" +
               $"{indent}    goto {NoBranchBlockId};";
    }
}

public class DelayBlock : Block
{
    public int DelayMilliseconds { get; }
    public string NextBlockId { get; }

    public DelayBlock(string blockId, int delayMilliseconds, string nextBlockId) : base(blockId)
    {
        DelayMilliseconds = delayMilliseconds;
        NextBlockId = Assign(nextBlockId);
    }

    public override string GenerateCode(int indentationLevel)
    {
        var indent = GetIndentation(indentationLevel);
        return $"{indent}{BlockId}:\n" +
               $"{indent}System.Threading.Thread.Sleep({DelayMilliseconds});\n" +
               $"{indent}goto {NextBlockId};";
    }
}

public class MathOperationBlock : Block
{
    public string TargetVariable { get; }
    public string BlockOperator { get; }
    public string OtherVariable { get; }
    public string NextBlockId { get; }

    public MathOperationBlock(string blockId, string targetVariable, string blockOperator, string otherVariable, string nextBlockId)
        : base(blockId)
    {
        TargetVariable = AssignTrimmed(targetVariable);
        OtherVariable = AssignTrimmed(otherVariable);
        BlockOperator = blockOperator;
        NextBlockId = Assign(nextBlockId);
    }

    public override string GenerateCode(int indentationLevel)
    {
        var indent = GetIndentation(indentationLevel);

        return $"{indent}{BlockId}:\n" +
               $"{indent}{TargetVariable} = {TargetVariable} {BlockOperator} {OtherVariable};\n" +
               $"{indent}goto {NextBlockId};";
    }
}