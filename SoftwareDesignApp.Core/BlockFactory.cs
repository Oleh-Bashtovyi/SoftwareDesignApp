using Newtonsoft.Json.Linq;

namespace SoftwareDesignApp.Core;

/// <summary>
/// Factory for creating blocks from JSON data
/// </summary>
public static class BlockFactory
{
    public static Block CreateBlock(JObject blockData)
    {
        if (blockData == null)
        {
            throw new ArgumentNullException(nameof(blockData), "Block data cannot be null");
        }

        if (!blockData.ContainsKey("type"))
        {
            throw new ArgumentException("Block data must contain 'type' field");
        }

        if (!blockData.ContainsKey("id"))
        {
            throw new ArgumentException("Block data must contain 'id' field");
        }

        string blockType = blockData["type"]!.ToString();
        string blockId = blockData["id"]!.ToString();

        switch (blockType)
        {
            case "StartBlock":
                string startnextBlockId = blockData["nextBlockId"]!.ToString();
                return new StartBlock(blockId, startnextBlockId);

            case "EndBlock":
                return new EndBlock(blockId);

            case "AssignmentBlock":
                ValidateField(blockData, "text", blockType);
                ValidateField(blockData, "nextBlockId", blockType);

                string fullText = blockData["text"]!.ToString();
                string nextBlockId = blockData["nextBlockId"]!.ToString();

                if (!fullText.Contains("="))
                {
                    throw new ArgumentException($"Assignment block must contain '=' in text: {fullText}");
                }

                string[] parts = fullText.Split('=');
                return new AssignmentBlock(blockId, parts[0].Trim(), parts[1].Trim(), nextBlockId);

            case "ConstantBlock":
                ValidateField(blockData, "text", blockType);
                ValidateField(blockData, "nextBlockId", blockType);

                string constText = blockData["text"]!.ToString();
                string constNextBlockId = blockData["nextBlockId"]!.ToString();

                if (!constText.Contains("="))
                {
                    throw new ArgumentException($"Constant block must contain '=' in text: {constText}");
                }

                string[] constParts = constText.Split('=');
                string varName = constParts[0].Trim();
                string valueStr = constParts[1].Trim();

                if (!int.TryParse(valueStr, out int value))
                {
                    throw new ArgumentException($"Constant block must have an integer value: {valueStr}");
                }

                return new ConstantBlock(blockId, varName, value, constNextBlockId);

            case "InputBlock":
                ValidateField(blockData, "text", blockType);
                ValidateField(blockData, "nextBlockId", blockType);

                string inputText = blockData["text"]!.ToString();
                string inputNextBlockId = blockData["nextBlockId"]!.ToString();

                if (!inputText.StartsWith("INPUT", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException($"Input block text must start with 'INPUT': {inputText}");
                }

                string inputVar = inputText.Replace("INPUT", "", StringComparison.OrdinalIgnoreCase).Trim();
                return new InputBlock(blockId, inputVar, inputNextBlockId);

            case "PrintBlock":
                ValidateField(blockData, "text", blockType);
                ValidateField(blockData, "nextBlockId", blockType);

                string printText = blockData["text"]!.ToString();
                string printNextBlockId = blockData["nextBlockId"]!.ToString();

                if (!printText.StartsWith("PRINT", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException($"Print block text must start with 'PRINT': {printText}");
                }

                string printVar = printText.Replace("PRINT", "", StringComparison.OrdinalIgnoreCase).Trim();
                return new PrintBlock(blockId, printVar, printNextBlockId);

            case "ConditionBlock":
                ValidateField(blockData, "text", blockType);
                ValidateField(blockData, "yesBranchBlockId", blockType);
                ValidateField(blockData, "noBranchBlockId", blockType);

                string condText = blockData["text"]!.ToString();
                string yesBranchBlockId = blockData["yesBranchBlockId"]!.ToString();
                string noBranchBlockId = blockData["noBranchBlockId"]!.ToString();

                string[] condParts = condText.Split(' ');
                if (condParts.Length < 3)
                {
                    throw new ArgumentException($"Condition block text must have format 'var condition value': {condText}");
                }

                return new ConditionBlock(
                    blockId,
                    condParts[0].Trim(),
                    condParts[1].Trim(),
                    condParts[2].Trim(),
                    yesBranchBlockId,
                    noBranchBlockId);

            default:
                throw new ArgumentException($"Unknown block type: {blockType}");
        }
    }

    private static void ValidateField(JObject blockData, string fieldName, string blockType)
    {
        if (!blockData.ContainsKey(fieldName))
        {
            throw new ArgumentException($"'{blockType}' block must contain '{fieldName}' field");
        }
    }
}