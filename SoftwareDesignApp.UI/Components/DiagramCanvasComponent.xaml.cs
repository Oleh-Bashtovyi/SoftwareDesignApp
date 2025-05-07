using SoftwareDesignApp.GUI.BlocksNew;
using SoftwareDesignApp.UI.Blocks;
using SoftwareDesignApp.UI.Blocks.Base;
using SoftwareDesignApp.UI.ViewModels;
using SoftwareDesignApp.UI.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using SoftwareDesignApp.UI.Exceptions;
using SoftwareDesignApp.UI.Enums;

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
        MenuItem addBlock = new MenuItem { Header = "Додати блок" };
        addBlock.Click += (s, e) => AddBlock();
        _contextMenu.Items.Add(deleteItem);
        _contextMenu.Items.Add(connectItem);
        _contextMenu.Items.Add(addBlock);
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

    private string GetBlockId()
    {
        string blockId = $"Block_{_blockIdCounter}";
        _blockIdCounter++;
        return blockId;
    }


    private void AddBlock()
    {
        var position = Mouse.GetPosition(MainCanvas);
        var parentWindow = Window.GetWindow(this);
        var blockType = Dialogs.ShowBlockTypeDialog(parentWindow, "Оберіть тип блоку для діаграми", "Тип блоку");

        if (blockType == null)
            return;

        try
        {
            BaseBlockControl? block = null;

            if (blockType == typeof(AssignmentBlockControl))
            {
                block = CreateAssignmentBlock();
            }
            else if (blockType == typeof(ConstantBlockControl))
            {
                block = CreateConstantBlock();
            }
            else if (blockType == typeof(InputBlockControl))
            {
                block = CreateInputBlock();
            }
            else if (blockType == typeof(PrintBlockControl))
            {
                block = CreatePrintBlock();
            }
            else if (blockType == typeof(ConditionBlockControl))
            {
                block = CreateConditionBlock();
            }
            else if (blockType == typeof(DelayBlockControl))
            {
                block = CreateDelayBlock();
            }
            else if (blockType == typeof(MathOperationBlockControl))
            {
                block = CreateMathOperationBlock();
            }
            else if (blockType == typeof(EndBlockControl))
            {
                block = new EndBlockControl(GetBlockId());
            }
            else if (blockType == typeof(StartBlockControl))
            {
                block = new StartBlockControl(GetBlockId());
            }
            else if (blockType == typeof(OneNextBlockControl))
            {
                block = new OneNextBlockControl(GetBlockId());
            }
            else
            {
                MessageBox.Show("Невірний тип блоку.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (block == null)
                return;

            AddBlock(block, position);
        }
        catch (DiagramException ex)
        {
            MessageBox.Show($"Помилка при створенні блоку: {DiagramErrorCodeToMessage(ex.ErrorCode)}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception)
        {
            MessageBox.Show("Помилка при створенні блоку.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private AssignmentBlockControl? CreateAssignmentBlock()
    {
        var parentWindow = Window.GetWindow(this);

        var firstAssignVariable = Dialogs.SelectVariableDialog(parentWindow, "Оберіть першу змінну", "Вибір змінної", SharedVariables);
        if (string.IsNullOrEmpty(firstAssignVariable)) return null;

        var secondAssignVariable = Dialogs.SelectVariableDialog(parentWindow, "Оберіть першу змінну", "Вибір змінної", SharedVariables);
        if (string.IsNullOrEmpty(secondAssignVariable)) return null;

        return new AssignmentBlockControl(GetBlockId(), firstAssignVariable, secondAssignVariable);
    }
    private ConstantBlockControl? CreateConstantBlock()
    {
        var parentWindow = Window.GetWindow(this);

        var variable = Dialogs.SelectVariableDialog(parentWindow, "Оберіть змінну для константного блоку", "Константний блок", SharedVariables);
        if (variable == null) return null;

        var str = Dialogs.ShowInputDialog(parentWindow, "Введіть початкове значення для змінної", "Ввести значення");
        if (str == null) return null;

        if (!int.TryParse(str, out int result))
        {
            MessageBox.Show("Введене значення не є цілим числом.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        return new ConstantBlockControl(GetBlockId(), variable, result);
    }
    private InputBlockControl? CreateInputBlock()
    {
        var parentWindow = Window.GetWindow(this);

        var inputVariable = Dialogs.SelectVariableDialog(parentWindow, "Оберіть змінну для вводу", "Блок вводу", SharedVariables);
        if (inputVariable == null) return null;
        
        return new InputBlockControl(GetBlockId(), inputVariable);
    }
    private PrintBlockControl? CreatePrintBlock()
    {
        var parentWindow = Window.GetWindow(this);
        var outputVariable = Dialogs.SelectVariableDialog(parentWindow, "Оберіть змінну для виводу", "Блок виводу", SharedVariables);
        if (outputVariable == null) return null;
        return new PrintBlockControl(GetBlockId(), outputVariable);
    }
    private ConditionBlockControl? CreateConditionBlock()
    {
        var parentWindow = Window.GetWindow(this);
        var condVariable = Dialogs.SelectVariableDialog(parentWindow, "Оберіть змінну для умови", "Блок умови", SharedVariables);
        if (condVariable == null) return null;
        var condValue = Dialogs.ShowInputDialog(parentWindow, "Введіть значення для порівняння", "Умова");
        if (condValue == null) return null;
        if (!int.TryParse(condValue, out int condInt))
        {
            MessageBox.Show("Значення має бути цілим числом", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
        var conditionItems = new List<GenericSelectDialog.ComboBoxItemModel>();
        conditionItems.Add(new("==", "=="));
        conditionItems.Add(new(">=", ">="));
        conditionItems.Add(new("<=", "<="));
        conditionItems.Add(new("<", "<"));
        conditionItems.Add(new(">", ">"));
        conditionItems.Add(new("!=", "!="));
        var conditionSign = Dialogs.SelectDialog<string>(parentWindow, conditionItems, "Введіть знак порівняння", "Умова");
        if (conditionSign == null) return null;
        return new ConditionBlockControl(GetBlockId(), condVariable, condInt, conditionSign);
    }
    private DelayBlockControl? CreateDelayBlock()
    {
        var parentWindow = Window.GetWindow(this);

        var str = Dialogs.ShowInputDialog(parentWindow, "Введіть затримку в мілісекундах", "Затримка");
        if (str == null) return null;
        if (!int.TryParse(str, out int result))
        {
            MessageBox.Show("Введене значення не є цілим числом.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
        if (result <= 0)
        {
            MessageBox.Show("Затримка не може бути від'ємною чи нульовою.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
        if (result > 10000)
        {
            MessageBox.Show("Затримка не може перевищувати 10 секунд.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        return new DelayBlockControl(GetBlockId(), result);
    }
    private MathOperationBlockControl? CreateMathOperationBlock()
    {
        var parentWindow = Window.GetWindow(this);
        var firstVariable = Dialogs.SelectVariableDialog(parentWindow, "Оберіть першу змінну", "Вибір змінної", SharedVariables);
        if (string.IsNullOrEmpty(firstVariable)) return null;

        var secondVariable = Dialogs.SelectVariableDialog(parentWindow, "Оберіть другу змінну", "Вибір змінної", SharedVariables);
        if (string.IsNullOrEmpty(secondVariable)) return null;

        var operationItems = new List<GenericSelectDialog.ComboBoxItemModel>
       {
           new("PLUS (+)", "+"),
           new("MINUS (-)", "-"),
           new("MULTIPLY (*)", "-"),
           new("DIVISION (/)", "/"),
           new("OR", "|"),
           new("AND", "&"),
           new("XOR", "^"),
       };

        var operation = Dialogs.SelectDialog<string>(parentWindow, operationItems, "Оберіть операцію (+, -, *, /)", "Операція");
        if (operation == null) return null;

        return new MathOperationBlockControl(GetBlockId(), firstVariable, operation, secondVariable);
    }
    private StartBlockControl CreateStartBlock()
    {
        return new StartBlockControl(GetBlockId());
    }
    private EndBlockControl CreateEndBlock()
    {
        return new EndBlockControl(GetBlockId());
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
            var nextBlock = Dialogs.SelectDialog<BaseBlockControl>(parentWindow, items, "Оберіть наступний блок:", "Оберіть блок");
            if (nextBlock == null) return;
            oneNextBlock.SetNextBlock(nextBlock);
            nextBlock.AddIncomingBlock(oneNextBlock);
            RedrawConnections();
        }
    }

    private void OnBlockMouseMove(object sender, MouseEventArgs e)
    {
        if (CurrentDragBlock != null && e.LeftButton == MouseButtonState.Pressed)
        {
            var position = e.GetPosition(MainCanvas);
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
            //AddBlock(sender, e);
            AddBlock();
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
            MainCanvas.Children.Remove(element);

        foreach (var element in MainCanvas.Children.OfType<Polygon>().ToList())
            MainCanvas.Children.Remove(element);

        // Перемальовуємо зв'язки для всіх блоків
        foreach (var block in ViewModel.GetBlocks())
        {
            double curBlockLeftPosition = Canvas.GetLeft(block);
            double curBlockTopPosition = Canvas.GetTop(block);

            if (block is OneNextBlockControl curOneNextBlock)
            {
                if (curOneNextBlock.NextBlock == null) continue;

                var nextBlockLeftPosition = Canvas.GetLeft(curOneNextBlock.NextBlock);
                var nextBlockTopPosition = Canvas.GetTop(curOneNextBlock.NextBlock);
                var y1Offset = curBlockTopPosition < nextBlockTopPosition ? 50 : 0;
                var y2Offset = curBlockTopPosition < nextBlockTopPosition ? 0 : 50;
                var line = new Line
                {
                    X1 = curBlockLeftPosition + 100,
                    Y1 = curBlockTopPosition + y1Offset,
                    X2 = nextBlockLeftPosition + 100,
                    Y2 = nextBlockTopPosition + y2Offset,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                AddArrowheadToLine(line);
                MainCanvas.Children.Add(line);
            }
            if (block is ConditionBlockControl curCondBlock)
            {
                if (curCondBlock.TrueConditionNextBlock == null) continue;
                var nextTrueBlockLeftPosition = Canvas.GetLeft(curCondBlock.TrueConditionNextBlock);
                var nextTrueBlockTopPosition = Canvas.GetTop(curCondBlock.TrueConditionNextBlock);
                var y1Offset = curBlockTopPosition < nextTrueBlockTopPosition ? 50 : 0;
                var y2Offset = curBlockTopPosition < nextTrueBlockTopPosition ? 0 : 50;
                var trueLine = new Line
                {
                    X1 = curBlockLeftPosition + 100,
                    Y1 = curBlockTopPosition + y1Offset,
                    X2 = nextTrueBlockLeftPosition + 100,
                    Y2 = nextTrueBlockTopPosition + y2Offset,
                    Stroke = Brushes.DarkGreen,
                    StrokeThickness = 1
                };
                AddArrowheadToLine(trueLine);
                MainCanvas.Children.Add(trueLine);

                if (curCondBlock.FalseConditionNextBlock == null) continue;
                var nextFalseBlockLeftPosition = Canvas.GetLeft(curCondBlock.FalseConditionNextBlock);
                var nextFalseBlockTopPosition = Canvas.GetTop(curCondBlock.FalseConditionNextBlock);
                y1Offset = curBlockTopPosition < nextFalseBlockTopPosition ? 50 : 0;
                y2Offset = curBlockTopPosition < nextFalseBlockTopPosition ? 0 : 50;
                var falseLine = new Line
                {
                    X1 = curBlockLeftPosition + 100,
                    Y1 = curBlockTopPosition + y1Offset,
                    X2 = nextFalseBlockLeftPosition + 100,
                    Y2 = nextFalseBlockTopPosition + y2Offset,
                    Stroke = Brushes.DarkRed,
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

    private string DiagramErrorCodeToMessage(DiagramErrorCode code)
    {
        return code switch
        {
            DiagramErrorCode.NoStartBlock => "Відсутній блок старту.",
            DiagramErrorCode.NoEndBlock => "Відсутній блок кінця.",
            DiagramErrorCode.MoreThanOneStartBlock => "Більше ніж один блок старту заборонено.",
            DiagramErrorCode.MoreThanOneEndBlock => "Більше ніж один блок кінця забороненно.",
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
        };
    }
}