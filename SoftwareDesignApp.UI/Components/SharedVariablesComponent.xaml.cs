using Microsoft.Win32;
using SoftwareDesignApp.Core;
using SoftwareDesignApp.UI.ViewModels;
using SoftwareDesignApp.UI.Windows;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

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
            MessageBox.Show(VariableErrorCodeToMessage(validationResult.ValidationErrorType));
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

    private void SaveVariables(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Оберіть файл для збереження спільних змінних",
            FileName = "SharedVariables.json",
            DefaultExt = ".json",
            Filter = "JSON і текстові файли (*.json, *.txt)|*.json;*.txt|Усі файли (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var json = JsonSerializer.Serialize(ViewModel.GetVariables());
                System.IO.File.WriteAllText(dialog.FileName, json);
                MessageBox.Show($"Спільні змінні збережено в {dialog.FileName}", "Збережено", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при збереженні: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void LoadVariables(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Оберіть файл для завантаження спільних змінних",
            Filter = "JSON і текстові файли (*.json, *.txt)|*.json;*.txt|Усі файли (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var json = System.IO.File.ReadAllText(dialog.FileName);
                var variables = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
                if (variables != null)
                {
                    foreach (var variable in variables)
                    {
                        VariableValidationException.ThrowIfInvalid(variable.Key);
                    }

                    ViewModel.Clear();
                    foreach (var variable in variables)
                    {
                        ViewModel.AddVariable(variable.Key, variable.Value);
                    }

                    MessageBox.Show($"Спільні змінні завантажено з {dialog.FileName}", "Завантажено", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (VariableValidationException ex)
            {
                MessageBox.Show($"Помика під час завантаження зміннної {ex.ValidationResult.Variable}: " +
                                $"{VariableErrorCodeToMessage(ex.ValidationResult.ValidationErrorType)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private string VariableErrorCodeToMessage(ValidationErrorType errorType)
    {
        return errorType switch
        {
            ValidationErrorType.EmptyName => "Назва змінної не може бути порожньою",
            ValidationErrorType.InvalidFirstChar => "Назва змінної повинна починатися з літери або символу підкреслення (_)",
            ValidationErrorType.InvalidChars => "Назва змінної може містити лише літери, цифри та символ підкреслення (_)",
            ValidationErrorType.ReservedKeyword => "Назва змінної не може бути зарезервованим словом C#",
            _ => "Невідома помилка при валідації назви змінної"
        };
    }
}