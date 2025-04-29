using SoftwareDesignApp.UI.Blocks;
using SoftwareDesignApp.UI.Blocks.Base;
using SoftwareDesignApp.UI.ViewModels;
using SoftwareDesignApp.UI.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SoftwareDesignApp.GUI.BlocksNew;
using System.Windows.Shapes;

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
        MenuItem connectItem = new MenuItem { Header = "З'єднати" };
        connectItem.Click += (s, e) => ConnectBlocksDialog();
        _contextMenu.Items.Add(deleteItem);
        _contextMenu.Items.Add(connectItem);
        ContextMenu = _contextMenu;

        foreach (var block in ViewModel.GetBlocks())
        {
            block.MouseLeftButtonDown += OnBlockMouseLeftButtonDown;
            block.MouseMove += OnBlockMouseMove;
            block.MouseLeftButtonUp += OnBlockMouseLeftButtonUp;
            block.MouseRightButtonDown += OnBlockMouseRightButtonDown;
            MainCanvas.Children.Add(block);
        }
        RedrawConnections();
    }

    private void AddBlock(object sender, MouseButtonEventArgs e)
    {
        //string blockId = (_blockIdCounter++ + 1).ToString();
        string blockId = $"Block_{_blockIdCounter}";
        _blockIdCounter++;
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

                    var conditionItems = new List<GenericSelectDialog.ComboBoxItemModel>();
                    conditionItems.Add(new("==", "=="));
                    conditionItems.Add(new(">=", ">="));
                    conditionItems.Add(new("<=", "<="));
                    conditionItems.Add(new("<", "<"));
                    conditionItems.Add(new(">", ">"));

                    var conditionSign = Dialogs.SelectDialog<string>(parentWindow, conditionItems,
                        "Введіть знак порівняння", "Умова");
                    if (conditionSign == null) return;

                    block = new ConditionBlockControl(blockId, condVariable, condInt, conditionSign);
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


    public void ConnectBlocksDialog()
    {
        if (CurrentSelectedBlock == null)
            return;

        if (CurrentSelectedBlock is EndBlockControl)
        {
            MessageBox.Show("Блок 'Кінець' не може мати наступних блоків.",
                "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Отримання доступних блоків для з'єднання
        List<BaseBlockControl> availableBlocks = ViewModel.GetBlocks()
            .Where(b => b != CurrentSelectedBlock && !(b is StartBlockControl))
            .ToList();

        if (availableBlocks.Count == 0)
        {
            MessageBox.Show("Немає доступних блоків для з'єднання.",
                "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var items = new List<GenericSelectDialog.ComboBoxItemModel>();

        foreach (var block in availableBlocks)
        {
            items.Add(new(block.GetDisplayText(), block));
        }

        if (CurrentSelectedBlock is ConditionBlockControl conditionBlock)
        {
            var parentWindow = Window.GetWindow(this);
            var trueConditionNextBlock =
                Dialogs.SelectDialog<BaseBlockControl>(parentWindow, items, "Оберіть блок для позитивної умови:",
                    "Оберіть блок");
            if (trueConditionNextBlock == null)
                return;

            var falseConditionNextBlock =
                Dialogs.SelectDialog<BaseBlockControl>(parentWindow, items, "Оберіть блок для негативної умови:",
                    "Оберіть блок");
            if (falseConditionNextBlock == null)
                return;

            if (trueConditionNextBlock == falseConditionNextBlock)
            {
                MessageBox.Show("Для умовного ьблока треба обрати 2 різні блоки.",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            conditionBlock.SetNextBlocks(trueConditionNextBlock, falseConditionNextBlock);
            trueConditionNextBlock.AddIncomingBlock(conditionBlock);
            falseConditionNextBlock.AddIncomingBlock(conditionBlock);
            RedrawConnections();
        }
        else if (CurrentSelectedBlock is OneNextBlockControl oneNextBlock)
        {
            var parentWindow = Window.GetWindow(this);
            var nextBlock =
                Dialogs.SelectDialog<BaseBlockControl>(parentWindow, items, "Оберіть наступний блок:",
                    "Оберіть блок");
            if (nextBlock == null)
                return;

            oneNextBlock.SetNextBlock(nextBlock);
            nextBlock.AddIncomingBlock(oneNextBlock);
            RedrawConnections();
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
        RedrawConnections();
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
            RedrawConnections();
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
            foreach (var block in CurrentSelectedBlock.GetIncomingBlocks())
            {
                block.RemoveConnection(CurrentSelectedBlock);
            }
            CurrentSelectedBlock = null;
            CurrentDragBlock = null;
            RedrawConnections();
        }
    }



    public void RedrawConnections()
    {
        // Видаляємо всі існуючі зв'язки
        foreach (var element in MainCanvas.Children.OfType<Line>().ToList())
        {
            MainCanvas.Children.Remove(element);
        }

        foreach (var element in MainCanvas.Children.OfType<Polygon>().ToList())
        {
            MainCanvas.Children.Remove(element);
        }

        // Перемальовуємо зв'язки для всіх блоків
        foreach (var block in ViewModel.GetBlocks())
        {
            double curBlockLeftPosition = Canvas.GetLeft(block);
            double curBlockTopPosition = Canvas.GetTop(block);

            if (block is OneNextBlockControl curOneNextBlock)
            {
                if (curOneNextBlock.NextBlock == null)
                    continue;

                double nextBlockLeftPosition = Canvas.GetLeft(curOneNextBlock.NextBlock);
                double nextBlockTopPosition = Canvas.GetTop(curOneNextBlock.NextBlock);

                Line line = new Line
                {
                    X1 = curBlockLeftPosition + 100,
                    Y1 = curBlockTopPosition + 50,
                    X2 = nextBlockLeftPosition + 100,
                    Y2 = nextBlockTopPosition,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                AddArrowheadToLine(line);
                MainCanvas.Children.Add(line);
            }
            if (block is ConditionBlockControl curCondBlock)
            {
                if (curCondBlock.TrueConditionNextBlock == null)
                    continue;

                double nextTrueBlockLeftPosition = Canvas.GetLeft(curCondBlock.TrueConditionNextBlock);
                double nextTrueBlockTopPosition = Canvas.GetTop(curCondBlock.TrueConditionNextBlock);

                Line trueLine = new Line
                {
                    X1 = curBlockLeftPosition + 100,
                    Y1 = curBlockTopPosition + 50,
                    X2 = nextTrueBlockLeftPosition + 100,
                    Y2 = nextTrueBlockTopPosition,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                AddArrowheadToLine(trueLine);
                MainCanvas.Children.Add(trueLine);

                if (curCondBlock.FalseConditionNextBlock == null)
                    continue;

                double nextFalseBlockLeftPosition = Canvas.GetLeft(curCondBlock.FalseConditionNextBlock);
                double nextFalseBlockTopPosition = Canvas.GetTop(curCondBlock.FalseConditionNextBlock);

                Line falseLine = new Line
                {
                    X1 = curBlockLeftPosition + 100,
                    Y1 = curBlockTopPosition + 50,
                    X2 = nextFalseBlockLeftPosition + 100,
                    Y2 = nextFalseBlockTopPosition,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                AddArrowheadToLine(falseLine);
                MainCanvas.Children.Add(falseLine);
            }
        }
    }

    private void AddArrowheadToLine(Line line)
    {
        // Створення стрілки на кінці лінії
        double arrowSize = 8;

        // Розрахунок кута лінії
        double deltaX = line.X2 - line.X1;
        double deltaY = line.Y2 - line.Y1;
        double angle = Math.Atan2(deltaY, deltaX);

        // Координати кінця лінії
        double x2 = line.X2;
        double y2 = line.Y2;

        // Створення трикутника для стрілки
        Polygon arrow = new Polygon();
        arrow.Points.Add(new Point(x2, y2));
        arrow.Points.Add(new Point(
            x2 - arrowSize * Math.Cos(angle - Math.PI / 6),
            y2 - arrowSize * Math.Sin(angle - Math.PI / 6)));
        arrow.Points.Add(new Point(
            x2 - arrowSize * Math.Cos(angle + Math.PI / 6),
            y2 - arrowSize * Math.Sin(angle + Math.PI / 6)));

        arrow.Fill = Brushes.Black;

        MainCanvas.Children.Add(arrow);
    }
}