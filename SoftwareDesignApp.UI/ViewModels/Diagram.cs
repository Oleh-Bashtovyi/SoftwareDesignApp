using SoftwareDesignApp.UI.Blocks.Base;
using System.Windows.Controls;
using System.Windows.Documents;
using SoftwareDesignApp.GUI.BlocksNew;
using SoftwareDesignApp.UI.Blocks;

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
        _blocks.Add(block);
    }

    public bool RemoveBlock(BaseBlockControl block)
    {
        return _blocks.Remove(block);
    }

    public IReadOnlyCollection<BaseBlockControl> GetBlocks() => _blocks;


    public Dictionary<string, object> ToDict()
    {
        var blocksList = new List<Dictionary<string, object?>>();

        var data = new Dictionary<string, object>
        {
            ["name"] = Name,
            ["blocks"] = blocksList,
            ["shared_variables"] = SharedVariables?.GetVariables() ?? new Dictionary<string, int>()
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

    public static Diagram LoadFromDict(Dictionary<string, object> data, SharedVariables sharedVariables)
    {
        var blockMap = new Dictionary<string, BaseBlockControl>();
        var name = data["name"].ToString();

        var diagram = new Diagram(name, sharedVariables);

        // Додаємо змінні у спільну пам'ять
        if (data.ContainsKey("shared_variables"))
        {
            var sharedVarsObj = data["shared_variables"];

            // Обробляємо різні можливі типи для shared_variables
            if (sharedVarsObj is Dictionary<string, object> sharedVarsDict)
            {
                foreach (var kvp in sharedVarsDict)
                {
                    // Конвертуємо значення до int
                    int value = Convert.ToInt32(kvp.Value);
                    sharedVariables.AddVariable(kvp.Key, value);
                }
            }
            else if (sharedVarsObj is Newtonsoft.Json.Linq.JObject jObject)
            {
                foreach (var prop in jObject.Properties())
                {
                    int value = prop.Value.ToObject<int>();
                    sharedVariables.AddVariable(prop.Name, value);
                }
            }
        }

        // Створення блоків
        if (data.ContainsKey("blocks"))
        {
            var blocksData = new List<Dictionary<string, object>>();

            // Обробляємо різні можливі типи для blocks
            if (data["blocks"] is List<object> blocksList)
            {
                foreach (var blockObj in blocksList)
                {
                    if (blockObj is Dictionary<string, object> blockDict)
                    {
                        blocksData.Add(blockDict);
                    }
                    else if (blockObj is Newtonsoft.Json.Linq.JObject jObj)
                    {
                        var blockDict2 = jObj.ToObject<Dictionary<string, object>>();
                        blocksData.Add(blockDict2);
                    }
                }
            }
            else if (data["blocks"] is Newtonsoft.Json.Linq.JArray jArray)
            {
                foreach (var item in jArray)
                {
                    var blockDict = item.ToObject<Dictionary<string, object>>();
                    blocksData.Add(blockDict);
                }
            }

            // Створюємо блоки
            foreach (var blockData in blocksData)
            {
                string blockType = blockData["type"].ToString();
                BaseBlockControl block = null;

                // Створюємо блок в залежності від його типу
                switch (blockType)
                {
                    case nameof(StartBlockControl):
                        block = new StartBlockControl(blockData["id"].ToString());
                        break;
                    case nameof(EndBlockControl):
                        block = new EndBlockControl(blockData["id"].ToString());
                        break;
                    case nameof(PrintBlockControl):
                        block = new PrintBlockControl(blockData["id"].ToString(), blockData["var"].ToString());
                        break;
                    case nameof(InputBlockControl):
                        block = new InputBlockControl(blockData["id"].ToString(), blockData["var"].ToString());
                        break;
                    case nameof(AssignmentBlockControl):
                        block = new AssignmentBlockControl(blockData["id"].ToString(),
                                                          blockData["var1"].ToString(),
                                                          blockData["var2"].ToString());
                        break;
                    case nameof(ConstantBlockControl):
                        block = new ConstantBlockControl(blockData["id"].ToString(),
                                                        blockData["var"].ToString(),
                                                        Convert.ToInt32(blockData["value"]));
                        break;
                    case nameof(ConditionBlockControl):
                        block = new ConditionBlockControl(blockData["id"].ToString(),
                                                         blockData["var"].ToString(),
                                                         Convert.ToInt32(blockData["value"]),
                                                         blockData["condition"].ToString());
                        break;
                }

                if (block != null)
                {
                    var x = Convert.ToDouble(blockData["x"]);
                    var y = Convert.ToDouble(blockData["y"]);
                    Canvas.SetLeft(block, x);
                    Canvas.SetTop(block, y);
                    blockMap[block.BlockId] = block;
                }
            }

            // Встановлюємо зв'язки між блоками
            foreach (var blockData in blocksData)
            {
                var blockId = blockData["id"].ToString();

                if (blockMap.TryGetValue(blockId, out var curBlock))
                {
                    if (curBlock is OneNextBlockControl oneNextBlockControl && blockData.ContainsKey("nextBlock"))
                    {
                        var nextId = blockData["nextBlock"].ToString();
                        if (!string.IsNullOrEmpty(nextId) && blockMap.TryGetValue(nextId, out var nextBlock))
                        {
                            oneNextBlockControl.SetNextBlock(nextBlock);
                        }
                    }

                    if (curBlock is ConditionBlockControl conditionBlockControl)
                    {
                        BaseBlockControl trueNextBlock = null;
                        BaseBlockControl falseNextBlock = null;

                        if (blockData.ContainsKey("trueNextBlock"))
                        {
                            var trueBlockId = blockData["trueNextBlock"].ToString();
                            if (!string.IsNullOrEmpty(trueBlockId) && blockMap.TryGetValue(trueBlockId, out var trueBlock))
                            {
                                trueNextBlock = trueBlock;
                            }
                        }

                        if (blockData.ContainsKey("falseNextBlock"))
                        {
                            var falseBlockId = blockData["falseNextBlock"].ToString();
                            if (!string.IsNullOrEmpty(falseBlockId) && blockMap.TryGetValue(falseBlockId, out var falseBlock))
                            {
                                falseNextBlock = falseBlock;
                            }
                        }

                        conditionBlockControl.SetNextBlocks(trueNextBlock, falseNextBlock);
                    }
                }
            }
        }

        foreach (var block in blockMap.Values)
        {
            diagram.AddBlock(block);
        }

        return diagram;
    }



}
