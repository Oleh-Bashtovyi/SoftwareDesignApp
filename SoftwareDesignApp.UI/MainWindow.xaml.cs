using SoftwareDesignApp.UI.Blocks;
using SoftwareDesignApp.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using SoftwareDesignApp.UI.Components;

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
        throw new NotImplementedException();
    }

    private void OpenFile(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void RunCode(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
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
}