using SoftwareDesignApp.UI.ViewModels;
using System.Windows;

namespace SoftwareDesignApp.UI.Windows;

public static class Dialogs
{
    public static string? ShowInputDialog(Window parent, string prompt, string title)
    {
        var window = new InputDialog(prompt, title)
        {
            Owner = parent
        };
        window.ShowDialog();
        return window.ResponseText;
    }

    public static Type? ShowBlockTypeDialog(Window parent, string prompt, string title)
    {
        var window = new SelectBlockTypeDialog(prompt, title)
        {
            Owner = parent
        };
        window.ShowDialog();
        return window.SelectedBlockType;
    }

    public static string? SelectVariableDialog(Window parent, string prompt, string title, SharedVariables sharedVariables)
    {
        var variables = sharedVariables.GetVariables();

        if (variables.Count == 0)
        {
            MessageBox.Show("Будь ласка, створіть принаймні одну спільну змінну перед використанням цього блоку.", 
                "Ще немає створених змінних", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        var window = new SelectVariableDialog(variables.Keys.ToList(), prompt, title)
        {
            Owner = parent
        };
        window.ShowDialog();
        return window.SelectedVariable;
    }

    public static T? SelectDialog<T>(Window parent, List<GenericSelectDialog.ComboBoxItemModel> items, string prompt, string title) where T : class
    {
        var genericDialog = new GenericSelectDialog(items, prompt, title);
        genericDialog.Owner = parent;
        genericDialog.ShowDialog();

        if (genericDialog.DialogResult != true)
            return null;

        return genericDialog.SelectedItem?.Id as T;
    }


}