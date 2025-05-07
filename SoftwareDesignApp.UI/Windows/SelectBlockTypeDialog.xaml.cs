using SoftwareDesignApp.GUI.BlocksNew;
using SoftwareDesignApp.UI.Blocks;
using System.Collections.ObjectModel;
using System.Windows;

namespace SoftwareDesignApp.UI.Windows;

public partial class SelectBlockTypeDialog : Window
{
    public class ComboBoxItemModel
    {
        public string Text { get; set; }
        public Type Id { get; set; }
    }

    public ObservableCollection<ComboBoxItemModel> BlockTypes { get; init; }

    public Type? SelectedBlockType { get; private set; }

    public ComboBoxItemModel? SelectedItem { get; set; }

    public SelectBlockTypeDialog(string prompt, string title = "Виберіть тип блоку")
    {
        InitializeComponent();
        PromptText.Text = prompt;
        Title = title;
        DataContext = this;

        BlockTypes =
        [
            new() { Text = "Присвоєння (A = B)", Id = typeof(AssignmentBlockControl) },
            new() { Text = "Константа (A = 5)", Id = typeof(ConstantBlockControl) },
            new() { Text = "Введення (INPUT A)", Id = typeof(InputBlockControl) },
            new() { Text = "Вивід (PRINT A)", Id = typeof(PrintBlockControl) },
            new() { Text = "Умова (A == B) або (A < B)", Id = typeof(ConditionBlockControl) },
            new() { Text = "Математичний (A += B) або (A /= B)", Id = typeof(MathOperationBlockControl) },
            new() { Text = "Затримка (DELAY 1000 ms)", Id = typeof(DelayBlockControl) },
            //new() { Text = "Кінець (End)", Id = typeof(EndBlockControl) }
        ];

        SelectedItem = BlockTypes[0];
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        SelectedBlockType = SelectedItem?.Id;
        DialogResult = true;
        Close();
    }
}