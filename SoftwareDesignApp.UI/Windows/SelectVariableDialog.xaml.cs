using System.Windows;

namespace SoftwareDesignApp.UI.Windows;

public partial class SelectVariableDialog : Window
{
    public string? SelectedVariable { get; private set; }

    public SelectVariableDialog(List<string> variables, string prompt, string title)
    {
        InitializeComponent();

        Title = title;
        PromptLabel.Content = prompt;
        VariableComboBox.ItemsSource = variables;

        if (variables.Count > 0)
            VariableComboBox.SelectedIndex = 0;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        SelectedVariable = VariableComboBox.SelectedItem as string;
        DialogResult = true;
        Close();
    }
}