using Microsoft.Win32;
using Newtonsoft.Json;
using SoftwareDesignApp.Core;
using SoftwareDesignApp.UI.Blocks;
using SoftwareDesignApp.UI.Components;
using SoftwareDesignApp.UI.ViewModels;
using SoftwareDesignApp.UI.Windows;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using SoftwareDesignApp.UI.Enums;
using SoftwareDesignApp.UI.Exceptions;

namespace SoftwareDesignApp.UI;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, DiagramCanvasComponent> _tabEditors = new();
    private readonly SharedVariables _sharedVariables;
    private int _pageCount;

    public MainWindow()
    {
        InitializeComponent();
        _sharedVariables = SharedVariablesComponent.ViewModel;
        _pageCount = 1;
        InitUi();
    }

    private void InitUi()
    {
        NewPage();
    }

    private void Save(object sender, RoutedEventArgs e)
    {
        if (tabs.Items.Count == 0)
            return;

        var selectedTab = tabs.SelectedItem as TabItem;
        if (selectedTab == null)
            return;

        var tabName = selectedTab.Header.ToString();
        if (_tabEditors.TryGetValue(tabName, out var canvasComponent))
        {
            var diagram = canvasComponent.ViewModel;

            var saveDialog = new SaveFileDialog
            {
                DefaultExt = ".json",
                Filter = "JSON Files (*.json)|*.json",
                FileName = $"{diagram.Name}.json"
            };

            if (saveDialog.ShowDialog() == true)
            {
                string json = JsonConvert.SerializeObject(diagram.ToDict(), Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(saveDialog.FileName, json);
                MessageBox.Show($"Діаграму збережено як {saveDialog.FileName}", "Збережено", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    private string GetNextDiagramName()
    {
        var name = $"Diagram_{_pageCount}";
        _pageCount++;
        return name;
    }

    private void OpenFile(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openDialog = new OpenFileDialog
        {
            Filter = "JSON Files (*.json)|*.json"
        };

        if (openDialog.ShowDialog() == true)
        {
            try
            {
                var json = File.ReadAllText(openDialog.FileName);
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                var diagramName = GetNextDiagramName();
                var diagram = Diagram.LoadFromDict(diagramName, data, _sharedVariables);

                TabItem newTab = new TabItem
                {
                    Header = diagramName
                };

                var editor = new DiagramCanvasComponent(diagram);
                newTab.Content = editor;
                tabs.Items.Add(newTab);
                tabs.SelectedItem = newTab;
                _tabEditors[diagramName] = editor;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при відкритті файлу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void RunCode(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Оберіть файл для запуску коду",
                DefaultExt = ".cs",
                Filter = "C# і текстові файли (*.cs, *.txt)|*.cs;*.txt|Усі файли (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                var compilationService = new CompilationService();
                var filePath = dialog.FileName;
                var code = File.ReadAllText(filePath);
                var result = compilationService.CompileAndExecuteFromString(code);

                if (result.Success)
                {
                    var window = new ResultWindow(result.Output);
                    window.ShowDialog();
                }
                else
                {
                    MessageBox.Show($"Виникла помилка під час компіляції: {result.ErrorMessage}");
                }
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show("Error occured: " + exception.Message);
        }
    }

    private void RunTest(object sender, RoutedEventArgs e)
    {
        var testWindow = new TestWindow();
        testWindow.ShowDialog();
    }

    private void ExitProgram(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
            "Зміни не зберігаються. Ви хочете зберегти зміни перед виходом?",
            "Вихід", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

        if (result == MessageBoxResult.Cancel)
            return;

        if (result == MessageBoxResult.Yes)
            Save(sender, e);

        Application.Current.Shutdown();
    }

    private void NewPage(object sender = null, RoutedEventArgs e = null)
    {
        var diagramName = GetNextDiagramName();
        var newTab = new TabItem
        {
            Header = diagramName
        };

        var diagramViewModel = new Diagram(diagramName, _sharedVariables);
        var editor = new DiagramCanvasComponent(diagramViewModel);

        // Додавання початкового блоку
        var startBlock = new StartBlockControl("Start");
        editor.AddBlock(startBlock);
        Canvas.SetLeft(startBlock, 250);
        Canvas.SetTop(startBlock, 50);
        var endBlock = new EndBlockControl("End");
        editor.AddBlock(endBlock);
        Canvas.SetLeft(endBlock, 250);
        Canvas.SetTop(endBlock, 300);

        newTab.Content = editor;
        tabs.Items.Add(newTab);
        tabs.SelectedItem = newTab;

        _tabEditors[diagramName] = editor;
    }

    private void DeletePage(object sender, RoutedEventArgs e)
    {
        if (tabs.Items.Count == 0 || tabs.SelectedItem == null)
            return;

        var selectedTab = tabs.SelectedItem as TabItem;
        if (selectedTab == null)
            return;

        var tabName = selectedTab.Header.ToString();
        if (tabName == null)
            return;

        MessageBoxResult result = MessageBox.Show(
            $"Видалити потік '{tabName}'?",
            "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            if (_tabEditors.TryGetValue(tabName, out DiagramCanvasComponent? editor))
            {
                tabs.Items.Remove(selectedTab);
                _tabEditors.Remove(tabName);
            }
        }
    }

    private void TranslateDiagramsToCode(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Title = "Оберіть файл для збереження трансльованого коду",
                FileName = "TranslatedResult.cs",
                DefaultExt = ".cs",
                Filter = "C# і текстові файли (*.cs, *.txt)|*.cs;*.txt|Усі файли (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                var filePath = dialog.FileName;
                var diagramThread = _tabEditors.Values.Select(x => x.ViewModel.ToDiagramThread()).ToList();
                var codeGenerator = new CodeGenerator();
                var code = codeGenerator.GenerateCode(diagramThread, _sharedVariables.GetVariables().ToDictionary());
                File.WriteAllText(filePath, code);
                MessageBox.Show("успішно трансльовано!");

            }
        }
        catch (DiagramException ex)
        {
            MessageBox.Show($"Помилка під час трансляції діаграми {ex.DiagramName}: {DiagramErrorCodeToMessage(ex.ErrorCode)}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception)
        {
            MessageBox.Show("Сталась помилка під час трансляції коду!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string DiagramErrorCodeToMessage(DiagramErrorCode code)
    {
        return code switch
        {
            DiagramErrorCode.NoStartBlock => "Відсутній блок старту.",
            DiagramErrorCode.NoEndBlock => "Відсутній блок кінця.",
            DiagramErrorCode.MoreThanOneStartBlock => "Більше ніж один блок старту.",
            DiagramErrorCode.MoreThanOneEndBlock => "Більше ніж один блок кінця.",
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
        };
    }
}