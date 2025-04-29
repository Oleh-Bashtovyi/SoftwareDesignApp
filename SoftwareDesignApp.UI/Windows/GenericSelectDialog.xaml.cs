using System.Collections.ObjectModel;
using System.Windows;

namespace SoftwareDesignApp.UI.Windows;

public partial class GenericSelectDialog : Window
{
    public class ComboBoxItemModel(string text, object id)
    {
        public string Text { get; } = text;
        public object Id { get; } = id;
    }

    public ObservableCollection<ComboBoxItemModel> Items { get; init; }

    public ComboBoxItemModel? SelectedItem { get; set; }

    public GenericSelectDialog(List<ComboBoxItemModel> items, string prompt, string title)
    {
        InitializeComponent();
        PromptText.Text = prompt;
        Title = title;
        Items = new(items);
        SelectedItem = Items[0];
        DataContext = this;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}



/*public partial class GenericSelectDialog<T> : Window where T : class
{
    public class ComboBoxItemModel<T>(string text, T id)
    {
        public string Text { get; } = text;
        public T Id { get; } = id;
    }

    public ObservableCollection<ComboBoxItemModel<T>> Items { get; init; }

    public ComboBoxItemModel<T>? SelectedItem { get; set; }

    public GenericSelectDialog(string prompt, string title = "Виберіть тип блоку")
    {
        InitializeComponent();
        PromptText.Text = prompt;
        Title = title;
        SelectedItem = Items[0];
        DataContext = this;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}*/