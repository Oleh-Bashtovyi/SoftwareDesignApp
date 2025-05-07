using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using SoftwareDesignApp.Core;
using SoftwareDesignApp.UI.ViewModels;
using SoftwareDesignApp.UI.Windows;

namespace SoftwareDesignApp.UI.Components;

public partial class SharedVariablesComponent : UserControl
{
    public SharedVariables ViewModel { get; }

    public SharedVariablesComponent()
    {
        InitializeComponent();
        ViewModel = new SharedVariables();
        DataContext = ViewModel;
    }

    private void AddVariable(object sender, RoutedEventArgs e)
    {
        var parentWindow = Window.GetWindow(this);
        var variableName = Dialogs.ShowInputDialog(parentWindow, "Введіть назву змінної:", "Ввести змінну");

        if (string.IsNullOrEmpty(variableName))
        {
            return;
        }

        var validationResult = VariableNameValidation.Validate(variableName);
        if (!validationResult.IsValid)
        {
            var errorMessage = validationResult.ValidationErrorType switch
            {
                ValidationErrorType.EmptyName => "Назва змінної не може бути порожньою",
                ValidationErrorType.InvalidFirstChar => "Назва змінної повинна починатися з літери або символу підкреслення (_)",
                ValidationErrorType.InvalidChars => "Назва змінної може містити лише літери, цифри та символ підкреслення (_)",
                ValidationErrorType.ReservedKeyword => "Назва змінної не може бути зарезервованим словом C#",
                _ => "Невідома помилка при валідації назви змінної"
            };

            MessageBox.Show(errorMessage);
            return;
        }

        var variableValue = Dialogs.ShowInputDialog(parentWindow, $"Введіть початкове значення для {variableName}:", "Додати значення");
        if (string.IsNullOrEmpty(variableValue))
        {
            return;
        }

        try
        {
            int value = int.Parse(variableValue);

            if (!ViewModel.AddVariable(variableName, value))
            {
                MessageBox.Show("Ім'я вже існує.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch
        {
            MessageBox.Show("Значення повинно бути цілим числом.",
                "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void EditVariable(object sender, RoutedEventArgs e)
    {
        var parentWindow = Window.GetWindow(this);
        var selectedVariable = Dialogs.SelectVariableDialog(parentWindow, "Оберіть змінну для редагування:",
            "Вибір змінної", ViewModel);

        if (string.IsNullOrEmpty(selectedVariable))
        {
            return;
        }

        var variableValue = Dialogs.ShowInputDialog(parentWindow, $"Введіть нове значення для {selectedVariable}:",
            "Нове значення");
        if (string.IsNullOrEmpty(variableValue))
        {
            return;
        }

        try
        {
            int value = int.Parse(variableValue);

            if (!ViewModel.UpdateVariable(selectedVariable, value))
            {
                MessageBox.Show("Змінної не існує.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch
        {
            MessageBox.Show("Значення повинно бути цілим числом.",
                "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DeleteVariable(object sender, RoutedEventArgs e)
    {
        var parentWindow = Window.GetWindow(this);
        var selectedVariable = Dialogs.SelectVariableDialog(parentWindow, "Оберіть змінну для видалення:",
            "Вибір змінної", ViewModel);

        if (string.IsNullOrEmpty(selectedVariable))
        {
            return;
        }

        ViewModel.RemoveVariable(selectedVariable);
    }
}