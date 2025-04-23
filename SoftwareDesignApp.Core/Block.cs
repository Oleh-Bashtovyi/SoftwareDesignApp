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
        TargetVariable = targetVariable?.Trim() ?? throw new ArgumentNullException(nameof(targetVariable));
        SourceVariable = sourceVariable?.Trim() ?? throw new ArgumentNullException(nameof(sourceVariable));
        NextBlockId = nextBlockId ?? throw new ArgumentNullException(nameof(nextBlockId));
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
        TargetVariable = targetVariable?.Trim() ?? throw new ArgumentNullException(nameof(targetVariable));
        Value = value;
        NextBlockId = nextBlockId ?? throw new ArgumentNullException(nameof(nextBlockId));
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
        TargetVariable = targetVariable?.Trim() ?? throw new ArgumentNullException(nameof(targetVariable));
        NextBlockId = nextBlockId ?? throw new ArgumentNullException(nameof(nextBlockId));
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
        SourceVariable = sourceVariable?.Trim() ?? throw new ArgumentNullException(nameof(sourceVariable));
        NextBlockId = nextBlockId ?? throw new ArgumentNullException(nameof(nextBlockId));
    }

    public override string GenerateCode(int indentationLevel)
    {
        var indent = GetIndentation(indentationLevel);
        return $"{indent}{BlockId}:\n" +
               $"{indent}Console.WriteLine(\"{SourceVariable} = \" + {SourceVariable});\n" +
               $"{indent}goto {NextBlockId};";
    }
}

/// <summary>
/// Condition block for branching (if V == C or if V < C)
/// </summary>
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
        Variable = variable?.Trim() ?? throw new ArgumentNullException(nameof(variable));
        Operator = operatorSymbol?.Trim() ?? throw new ArgumentNullException(nameof(operatorSymbol));
        Value = value?.Trim() ?? throw new ArgumentNullException(nameof(value));
        YesBranchBlockId = yesBranchBlockId ?? throw new ArgumentNullException(nameof(yesBranchBlockId));
        NoBranchBlockId = noBranchBlockId ?? throw new ArgumentNullException(nameof(noBranchBlockId));
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