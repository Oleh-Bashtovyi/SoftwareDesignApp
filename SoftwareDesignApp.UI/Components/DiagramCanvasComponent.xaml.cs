using SoftwareDesignApp.UI.Blocks;
using SoftwareDesignApp.UI.Blocks.Base;
using SoftwareDesignApp.UI.ViewModels;
using SoftwareDesignApp.UI.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SoftwareDesignApp.GUI.BlocksNew;

namespace SoftwareDesignApp.UI.Components;

public partial class DiagramCanvasComponent : UserControl
{
    private readonly ContextMenu _contextMenu;
    private int _blockIdCounter = 1;
    private BaseBlockControl? _currentSelectedBlock;
    private BaseBlockControl? _currentDragBlock;
    private Point _dragOffset;

    public Diagram ViewModel { get; }
    public SharedVariables SharedVariables => ViewModel.SharedVariables;

    private BaseBlockControl? CurrentSelectedBlock
    {
        get => _currentSelectedBlock;
        set
        {
            if (_currentSelectedBlock == value)
                return;

            if (_currentSelectedBlock != null)
                _currentSelectedBlock.UnMarkAsSelected();

            _currentSelectedBlock = value;

            if (_currentSelectedBlock != null)
                _currentSelectedBlock.MarkAsSelected();
        }
    }

    private BaseBlockControl? CurrentDragBlock
    {
        get => _currentDragBlock;
        set
        {
            if (_currentDragBlock == value)
                return;

            _currentDragBlock = value;
            CurrentSelectedBlock = value;
        }
    }

    public DiagramCanvasComponent(Diagram viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
        MainCanvas.Background = Brushes.White;
        MainCanvas.MouseLeftButtonDown += OnCanvasLeftButtonDown;
        MainCanvas.MouseLeftButtonUp += OnCanvasLeftButtonUp;

        _contextMenu = new ContextMenu();
        MenuItem deleteItem = new MenuItem { Header = "Видалити" };
        deleteItem.Click += (s, e) => DeleteSelectedBlock();
        _contextMenu.Items.Add(deleteItem);
        ContextMenu = _contextMenu;

        foreach (var block in ViewModel.GetBlocks())
        {
            block.MouseLeftButtonDown += OnBlockMouseLeftButtonDown;
            block.MouseMove += OnBlockMouseMove;
            block.MouseLeftButtonUp += OnBlockMouseLeftButtonUp;
            block.MouseRightButtonDown += OnBlockMouseRightButtonDown;
            MainCanvas.Children.Add(block);
        }
    }

    private void AddBlock(object sender, MouseButtonEventArgs e)
    {
        string blockId = (_blockIdCounter++ + 1).ToString();
        Point position = e.GetPosition(this);

        var parentWindow = Window.GetWindow(this);
        var blockType = Dialogs.ShowBlockTypeDialog(parentWindow, "Оберіть тип блоку для діаграми", "Тип блоку");

        if (blockType == null)
            return;

        try
        {
            BaseBlockControl block;

            switch (blockType)
            {
                case Type t when t == typeof(AssignmentBlockControl):
                    var firstAssignVariable = Dialogs.SelectVariableDialog(parentWindow, "Оберіть першу змінну", "Вибір змінної", SharedVariables);
                    if (string.IsNullOrEmpty(firstAssignVariable)) return;

                    var secondAssignVariable = Dialogs.SelectVariableDialog(parentWindow, "Оберіть першу змінну", "Вибір змінної", SharedVariables);
                    if (string.IsNullOrEmpty(secondAssignVariable)) return;

                    block = new AssignmentBlockControl(blockId, firstAssignVariable, secondAssignVariable);
                    break;

                case Type t when t == typeof(ConstantBlockControl):
                    var variable = Dialogs.SelectVariableDialog(parentWindow, "Оберіть змінну для константного блоку", "Константний блок", SharedVariables);
                    if (variable == null) return;

                    var str = Dialogs.ShowInputDialog(parentWindow, "Введіть початкове значення для змінної", "Ввести значення");
                    if (str == null) return;

                    if (!int.TryParse(str, out int result))
                    {
                        MessageBox.Show("Введене значення не є цілим числом.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    block = new ConstantBlockControl(blockId, variable, result);
                    break;

                case Type t when t == typeof(InputBlockControl):
                    var inputVariable = Dialogs.SelectVariableDialog(parentWindow, "Оберіть змінну для вводу", "Блок вводу", SharedVariables);
                    if (inputVariable == null) return;
                    block = new InputBlockControl(blockId, inputVariable);
                    break;

                case Type t when t == typeof(PrintBlockControl):
                    var outputVariable = Dialogs.SelectVariableDialog(parentWindow, "Оберіть змінну для виводу", "Блок виводу", SharedVariables);
                    if (outputVariable == null) return;
                    block = new PrintBlockControl(blockId, outputVariable);
                    break;

                case Type t when t == typeof(ConditionBlockControl):
                    var condVariable = Dialogs.SelectVariableDialog(parentWindow, "Оберіть змінну для умови", "Блок умови", SharedVariables);
                    if (condVariable == null) return;

                    var condValue = Dialogs.ShowInputDialog(parentWindow, "Введіть значення для порівняння", "Умова");
                    if (condValue == null) return;

                    if (!int.TryParse(condValue, out int condInt))
                    {
                        MessageBox.Show("Значення має бути цілим числом", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    block = new ConditionBlockControl(blockId, condVariable, condInt, "==");
                    break;

                case Type t when t == typeof(EndBlockControl):
                    block = new EndBlockControl(blockId);
                    break;

                default:
                    MessageBox.Show("Тип блоку не підтримується", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
            }

            AddBlock(block, position);
        }
        catch (ArgumentException)
        {
            // Обробка винятку
        }
    }

    public void AddBlock(BaseBlockControl block, Point point) => AddBlock(block, point.X, point.Y);
    public void AddBlock(BaseBlockControl block, double x = 0, double y = 0)
    {
        block.MouseLeftButtonDown += OnBlockMouseLeftButtonDown;
        block.MouseMove += OnBlockMouseMove;
        block.MouseLeftButtonUp += OnBlockMouseLeftButtonUp;
        block.MouseRightButtonDown += OnBlockMouseRightButtonDown;
        ViewModel.AddBlock(block);
        MainCanvas.Children.Add(block);
        Canvas.SetLeft(block, x);
        Canvas.SetTop(block, y);
    }

    private void OnBlockMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        CurrentDragBlock = sender as BaseBlockControl;

        if (CurrentDragBlock != null)
        {
            ContextMenu!.IsOpen = true;
        }
    }

    private void OnBlockMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        CurrentDragBlock = sender as BaseBlockControl;

        if (CurrentDragBlock != null)
        {
            _dragOffset = e.GetPosition(CurrentDragBlock);
            CurrentDragBlock.CaptureMouse();
        }
    }

    private void OnBlockMouseMove(object sender, MouseEventArgs e)
    {
        if (CurrentDragBlock != null && e.LeftButton == MouseButtonState.Pressed)
        {
            var position = e.GetPosition(this);
            Canvas.SetLeft(CurrentDragBlock, position.X - _dragOffset.X);
            Canvas.SetTop(CurrentDragBlock, position.Y - _dragOffset.Y);
        }
    }

    private void OnBlockMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (CurrentDragBlock != null)
        {
            CurrentDragBlock.ReleaseMouseCapture();
            CurrentDragBlock = null;
        }
    }

    private void OnCanvasLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount > 1)
        {
            AddBlock(sender, e);
        }
    }

    private void OnCanvasLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        CurrentDragBlock = null;
    }

    public void DeleteSelectedBlock()
    {
        if (CurrentSelectedBlock != null)
        {
            ViewModel.RemoveBlock(CurrentSelectedBlock);
            MainCanvas.Children.Remove(CurrentSelectedBlock);
            CurrentSelectedBlock.ReleaseMouseCapture();
            CurrentSelectedBlock = null;
            CurrentDragBlock = null;
        }
    }
}