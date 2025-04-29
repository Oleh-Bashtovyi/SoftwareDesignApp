using System.IO;
using SoftwareDesignApp.UI.Blocks;
using SoftwareDesignApp.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using SoftwareDesignApp.Core;
using SoftwareDesignApp.UI.Components;
using SoftwareDesignApp.UI.Windows;
using Formatting = System.Xml.Formatting;

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
        _pageCount = 0;
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
                string json = File.ReadAllText(openDialog.FileName);
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                var diagram = Diagram.LoadFromDict(data, _sharedVariables);

                string diagramName = diagram.Name;
                string uniqueName = diagramName;
                int counter = 1;

                while (_tabEditors.ContainsKey(uniqueName))
                {
                    uniqueName = $"{diagramName}_{counter}";
                    counter++;
                }

                TabItem newTab = new TabItem
                {
                    Header = uniqueName
                };

                var editor = new DiagramCanvasComponent(diagram);
                newTab.Content = editor;
                tabs.Items.Add(newTab);
                tabs.SelectedItem = newTab;
                _tabEditors[uniqueName] = editor;
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
            var dialog = new SaveFileDialog
            {
                Title = "Оберіть файл для запуску коду",
                DefaultExt = ".cs",
                Filter = "C# і текстові файли (*.cs, *.txt)|*.cs;*.txt|Усі файли (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                var compilationService = new CompilationService();
                string filePath = dialog.FileName;
                string code = File.ReadAllText(filePath);
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
        throw new NotImplementedException();
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
        _pageCount++;
        var diagramName = $"Diagram_{_pageCount}";
        var uniqueName = diagramName;
        var counter = 1;

        while (_tabEditors.ContainsKey(uniqueName))
        {
            uniqueName = $"{diagramName}_{counter}";
            counter++;
        }

        var newTab = new TabItem
        {
            Header = uniqueName
        };

        var diagramViewModel = new Diagram(uniqueName, _sharedVariables);
        var editor = new DiagramCanvasComponent(diagramViewModel);

        // Додавання початкового блоку
        var startBlock = new StartBlockControl("1");
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

        _tabEditors[uniqueName] = editor;
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

    private void tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
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
                string filePath = dialog.FileName;


                var diagramThread = _tabEditors.Values.Select(x => x.ViewModel.ToDiagramThread()).ToList();
                var codeGenerator = new CodeGenerator();
                var code = codeGenerator.GenerateCode(diagramThread); 
                File.WriteAllText(filePath, code);
                MessageBox.Show("успішно трансльовано!");

            }
        }
        catch (Exception exception)
        {
            MessageBox.Show("Error occured: " + exception.Message);
        }
    }
}