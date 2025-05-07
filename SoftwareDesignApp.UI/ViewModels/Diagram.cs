using SoftwareDesignApp.GUI.BlocksNew;
using SoftwareDesignApp.UI.Blocks;
using SoftwareDesignApp.UI.Blocks.Base;
using System.Windows.Controls;
using SoftwareDesignApp.Core;
using SoftwareDesignApp.UI.Enums;
using SoftwareDesignApp.UI.Exceptions;

namespace SoftwareDesignApp.UI.ViewModels;

public class Diagram(string name, SharedVariables sharedVariables) : BaseViewModel
{
    private readonly List<BaseBlockControl> _blocks = [];
    private SharedVariables _sharedVariables = sharedVariables;
    private string _name = name;

    public SharedVariables SharedVariables
    {
        get => _sharedVariables;
        private set => SetField(ref _sharedVariables, value);
    }
    
    public string Name
    {
        get => _name;
        private set => SetField(ref _name, value);
    }

    public void AddBlock(BaseBlockControl block)
    {
        if (block == null)
            throw new ArgumentNullException(nameof(block));

        if (block is StartBlockControl)
        {
            if (_blocks.OfType<StartBlockControl>().Any())
                throw new DiagramException(Name, DiagramErrorCode.MoreThanOneStartBlock, "Only one start block is allowed.");
        }

        if (block is EndBlockControl)
        {
            if (_blocks.OfType<EndBlockControl>().Any())
                throw new DiagramException(Name, DiagramErrorCode.MoreThanOneEndBlock, "Only one end block is allowed.");
        }

        _blocks.Add(block);
    }

    public bool RemoveBlock(BaseBlockControl block)
    {
        return _blocks.Remove(block);
    }

    public IReadOnlyCollection<BaseBlockControl> GetBlocks() => _blocks;


    public DiagramThread ToDiagramThread()
    {
        ThrowIfDiagramInvalid();
        var endBlock = _blocks.OfType<EndBlockControl>().FirstOrDefault();
        var diagramThread = new DiagramThread(Name, _blocks.Select(x => x.ToCoreBlock(endBlock)).ToList());
        return diagramThread;
    }

    private void ThrowIfDiagramInvalid()
    {
        var startBlocksCount = _blocks.OfType<StartBlockControl>().Count();
        if (startBlocksCount == 0)
            throw new DiagramException(Name, DiagramErrorCode.NoStartBlock, "Diagram does not contain start block");
        if (startBlocksCount > 1)
            throw new DiagramException(Name, DiagramErrorCode.MoreThanOneStartBlock, "Diagram contains more than one start block");

        var endBlocksCount = _blocks.OfType<EndBlockControl>().Count();
        if (endBlocksCount == 0)
            throw new DiagramException(Name, DiagramErrorCode.NoEndBlock, "Diagram does not contain end block");
        if (endBlocksCount > 1)
            throw new DiagramException(Name, DiagramErrorCode.MoreThanOneEndBlock, "Diagram contains more than one end block");
    }


    public Dictionary<string, object> ToDict()
    {
        var blocksList = new List<Dictionary<string, object?>>();

        var data = new Dictionary<string, object>
        {
            ["blocks"] = blocksList,
        };

        foreach (var block in _blocks)
        {
            var blockData = new Dictionary<string, object?>
            {
                ["id"] = block.BlockId,
                ["type"] = block.GetType().Name,
                ["x"] = Canvas.GetLeft(block),
                ["y"] = Canvas.GetTop(block),
            };

            if (block is PrintBlockControl printBlock)
            {
                blockData["var"] = printBlock.Variable;
            }
            else if (block is AssignmentBlockControl assignmentBlock)
            {
                blockData["var1"] = assignmentBlock.Var1;
                blockData["var2"] = assignmentBlock.Var2;
            }
            else if (block is ConstantBlockControl constantBlock)
            {
                blockData["var"] = constantBlock.Var;
                blockData["value"] = constantBlock.Value;
            }
            else if (block is InputBlockControl inputBlock)
            {
                blockData["var"] = inputBlock.Var;
            }
            else if (block is ConditionBlockControl conditionBlock)
            {
                blockData["var"] = conditionBlock.Var;
                blockData["value"] = conditionBlock.Value;
                blockData["condition"] = conditionBlock.Condition;
            }
            else if (block is DelayBlockControl delayBlock)
            {
                blockData["delay"] = delayBlock.DelayMs;
            }
            else if (block is MathOperationBlockControl mathOperationBlock)
            {
                blockData["var1"] = mathOperationBlock.TargetVariable;
                blockData["var2"] = mathOperationBlock.OperationVariable;
                blockData["operation"] = mathOperationBlock.Operation;
            }

            if (block is OneNextBlockControl oneNextBlockControl && oneNextBlockControl.NextBlock != null)
            {
                blockData["nextBlock"] = oneNextBlockControl.NextBlock.BlockId;
            }
            else if (block is ConditionBlockControl conditionBlockControl)
            {
                blockData["trueNextBlock"] = conditionBlockControl.TrueConditionNextBlock?.BlockId;
                blockData["falseNextBlock"] = conditionBlockControl.FalseConditionNextBlock?.BlockId;
            }

            blocksList.Add(blockData);
        }

        return data;
    }

    public static Diagram LoadFromDict(string diagramName, Dictionary<string, object> data, SharedVariables sharedVariables)
    {
        var blockMap = new Dictionary<string, BaseBlockControl>();
        var diagram = new Diagram(diagramName, sharedVariables);

        if (data.ContainsKey("blocks"))
        {
            var blocksData = ParseBlocksData(data["blocks"]);

            // Створюємо блоки
            foreach (var blockData in blocksData)
            {
                string blockId = blockData["id"].ToString();
                string blockType = blockData["type"].ToString();
                double x = Convert.ToDouble(blockData["x"]);
                double y = Convert.ToDouble(blockData["y"]);

                var block = CreateBlock(blockType, blockData);

                if (block != null)
                {
                    Canvas.SetLeft(block, x);
                    Canvas.SetTop(block, y);
                    blockMap[blockId] = block;
                }
            }

            // Встановлюємо зв'язки між блоками
            SetupBlockConnections(blocksData, blockMap);
        }

        foreach (var block in blockMap.Values)
        {
            diagram.AddBlock(block);
        }

        return diagram;
    }

    private static List<Dictionary<string, object>> ParseBlocksData(object blocksObj)
    {
        var blocksData = new List<Dictionary<string, object>>();

        if (blocksObj is List<object> blocksList)
        {
            foreach (var blockObj in blocksList)
            {
                AddBlockToDictionary(blockObj, blocksData);
            }
        }
        else if (blocksObj is Newtonsoft.Json.Linq.JArray jArray)
        {
            foreach (var item in jArray)
            {
                var blockDict = item.ToObject<Dictionary<string, object>>();
                if (blockDict != null)
                    blocksData.Add(blockDict);
            }
        }

        return blocksData;
    }

    private static void AddBlockToDictionary(object blockObj, List<Dictionary<string, object>> blocksData)
    {
        if (blockObj is Dictionary<string, object> blockDict)
        {
            blocksData.Add(blockDict);
        }
        else if (blockObj is Newtonsoft.Json.Linq.JObject jObj)
        {
            var blockDict2 = jObj.ToObject<Dictionary<string, object>>();
            if (blockDict2 != null)
                blocksData.Add(blockDict2);
        }
    }

    private static BaseBlockControl CreateBlock(string blockType, Dictionary<string, object> blockData)
    {
        string id = blockData["id"].ToString();

        switch (blockType)
        {
            case nameof(StartBlockControl):
                return new StartBlockControl(id);

            case nameof(EndBlockControl):
                return new EndBlockControl(id);

            case nameof(PrintBlockControl):
                return new PrintBlockControl(id, blockData["var"].ToString());

            case nameof(InputBlockControl):
                return new InputBlockControl(id, blockData["var"].ToString());

            case nameof(AssignmentBlockControl):
                return new AssignmentBlockControl(id, blockData["var1"].ToString(), blockData["var2"].ToString());

            case nameof(ConstantBlockControl):
                return new ConstantBlockControl(id, blockData["var"].ToString(), Convert.ToInt32(blockData["value"]));

            case nameof(ConditionBlockControl):
                return new ConditionBlockControl(id, blockData["var"].ToString(),
                    Convert.ToInt32(blockData["value"]), blockData["condition"].ToString());

            case nameof(DelayBlockControl):
                return new DelayBlockControl(id, Convert.ToInt32(blockData["delay"]));

            case nameof(MathOperationBlockControl):
                return new MathOperationBlockControl(id, blockData["var1"].ToString(),
                    blockData["operation"].ToString(), blockData["var2"].ToString());

            default:
                return null;
        }
    }

    private static void SetupBlockConnections(List<Dictionary<string, object>> blocksData, Dictionary<string, BaseBlockControl> blockMap)
    {
        foreach (var blockData in blocksData)
        {
            var blockId = blockData["id"].ToString();

            if (!blockMap.TryGetValue(blockId, out var curBlock))
                continue;

            // Встановлюємо зв'язки для блоків з одним виходом
            if (curBlock is OneNextBlockControl oneNextBlock && blockData.ContainsKey("nextBlock"))
            {
                var nextId = blockData["nextBlock"]?.ToString();
                if (!string.IsNullOrEmpty(nextId) && blockMap.TryGetValue(nextId, out var nextBlock))
                {
                    oneNextBlock.SetNextBlock(nextBlock);
                    nextBlock?.AddIncomingBlock(oneNextBlock);
                }
            }

            // Встановлюємо зв'язки для умовних блоків
            if (curBlock is ConditionBlockControl conditionBlock)
            {
                SetupConditionBlockConnections(blockData, blockMap, conditionBlock);
            }
        }
    }

    private static void SetupConditionBlockConnections(Dictionary<string, object> blockData,
        Dictionary<string, BaseBlockControl> blockMap, ConditionBlockControl conditionBlock)
    {
        BaseBlockControl trueNextBlock = null;
        BaseBlockControl falseNextBlock = null;

        if (blockData.ContainsKey("trueNextBlock"))
        {
            var trueBlockId = blockData["trueNextBlock"]?.ToString();
            if (!string.IsNullOrEmpty(trueBlockId))
                blockMap.TryGetValue(trueBlockId, out trueNextBlock);
        }

        if (blockData.ContainsKey("falseNextBlock"))
        {
            var falseBlockId = blockData["falseNextBlock"]?.ToString();
            if (!string.IsNullOrEmpty(falseBlockId))
                blockMap.TryGetValue(falseBlockId, out falseNextBlock);
        }

        conditionBlock.SetNextBlocks(trueNextBlock, falseNextBlock);
        trueNextBlock?.AddIncomingBlock(conditionBlock);
        falseNextBlock?.AddIncomingBlock(conditionBlock);
    }
}
