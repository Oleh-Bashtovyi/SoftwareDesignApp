using System.Windows;

namespace SoftwareDesignApp.UI.Windows;

public partial class InputDialog : Window
{
    public string? ResponseText { get; private set; }

    public InputDialog(string prompt, string title)
    {
        InitializeComponent();

        Title = title;
        PromptLabel.Content = prompt;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        ResponseText = InputTextBox.Text;
        DialogResult = true;
        Close();
    }
}