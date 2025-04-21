using System.Collections.ObjectModel;

namespace SoftwareDesignApp.UI.ViewModels;
public class SharedVariables(int maxVariablesCount = 100)
{
    private readonly Dictionary<string, int> _variables = new();

    public ObservableCollection<VariableModel> VariablesCollection { get; } = new();

    public int MaxVariablesCount { get; } = maxVariablesCount;

    public bool AddVariable(string name, int value = 0)
    {
        if (_variables.Count >= MaxVariablesCount || !_variables.TryAdd(name, value))
        {
            return false;
        }

        VariablesCollection.Add(new VariableModel(name, value));
        return true;
    }

    public bool RemoveVariable(string name)
    {
        if (!_variables.Remove(name))
        {
            return false;
        }

        var toRemove = VariablesCollection.FirstOrDefault(v => v.Name == name);
        if (toRemove != null)
        {
            VariablesCollection.Remove(toRemove);
        }

        return true;
    }

    public bool UpdateVariable(string name, int newValue)
    {
        if (!_variables.ContainsKey(name))
            return false;

        _variables[name] = newValue;

        var item = VariablesCollection.FirstOrDefault(v => v.Name == name);
        if (item != null)
            item.Value = newValue;

        return true;
    }

    public IReadOnlyDictionary<string, int> GetVariables() => _variables;
}