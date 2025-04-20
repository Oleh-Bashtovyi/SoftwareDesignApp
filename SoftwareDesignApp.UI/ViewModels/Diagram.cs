using SoftwareDesignApp.UI.Blocks.Base;

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
}
