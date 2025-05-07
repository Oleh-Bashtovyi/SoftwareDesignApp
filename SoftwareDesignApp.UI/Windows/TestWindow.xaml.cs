using Microsoft.Win32;
using SoftwareDesignApp.Core;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace SoftwareDesignApp.UI.Windows;

public class LogEntry(DateTime timeStamp, string message)
{
    public DateTime Timestamp { get; } = timeStamp;
    public string Message { get; } = message;
}


public partial class TestWindow : Window
{
    private readonly TestManager _testManager = new();
    private int _kValue = 10; 
    private string? _codePath;
    private List<TestCase> _testCases = [];
    private List<LogEntry> _logs = [];

    public TestWindow()
    {
        InitializeComponent();
    }

    private void LoadTests_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Directory.GetCurrentDirectory(),
                Title = "Select JSON test file",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                var content = File.ReadAllText(filePath);
                var loadedTestCases = JsonSerializer.Deserialize<List<TestCase>>(content);

                if (loadedTestCases != null)
                {
                    _testCases = loadedTestCases;
                    AddLog($"Loaded tests from: {filePath}");
                    StatusLabel.Content = $"Tests loaded: {Path.GetFileName(filePath)}";
                }
                else
                {
                    MessageBox.Show("Failed to load tests. Please check the file format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show($"Error loading tests: {exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    private void RunTests_Click(object sender, RoutedEventArgs e)
    {
        if (!_testCases.Any())
        {
            AddLog("No tests loaded. Please load tests first.");
            return;
        }
        if (string.IsNullOrEmpty(_codePath))
        {
            AddLog("No code file loaded. Please load a code file to test.");
            return;
        }

        AddLog("Starting tests...");
        try
        {
            var code = File.ReadAllText(_codePath);
            var testResults = _testManager.RunTests(code, _testCases, _kValue);
            int passedCount = 0;
            int totalVariants = 0;

            foreach (var testData in testResults)
            {
                totalVariants += testData.Attempts;
                passedCount += testData.SuccessCount;
            }

            var coveragePercent = (double)passedCount / totalVariants * 100;

            string summary = $"Test Summary:\n" +
                $"Passed (OK): {passedCount} / {totalVariants}\n" +
                $"Coverage for <= {_kValue} steps: {coveragePercent:F2}%";
            AddLog(summary);
        }
        catch (Exception ex)
        {
            AddLog($"Error during testing: {ex.Message}");
        }
    }

    private void LoadCode_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Title = "Select code file to test",
            Filter = "C# і текстові файли (*.cs, *.txt)|*.cs;*.txt|Усі файли (*.*)|*.*",
            DefaultExt = ".cs",
            InitialDirectory = Directory.GetCurrentDirectory()
        };

        if (openFileDialog.ShowDialog() == true)
        {
            _codePath = openFileDialog.FileName;
            AddLog($"Loaded code file: {_codePath}");
            StatusLabel.Content = $"Code loaded: {Path.GetFileName(_codePath)}";
        }
    }

    private void SetK_Click(object sender, RoutedEventArgs e)
    {
        var parentWindow = Window.GetWindow(this);
        var inputValue = Dialogs.ShowInputDialog(parentWindow, "Enter K (1 <= K <= 20):", "Set K");

        if (inputValue == null)
            return;

        try
        {
            int value = int.Parse(inputValue);
            if (value is >= 1 and <= 20)
            {
                _kValue = value;
                AddLog($"Set K to {_kValue}");
            }
            else
            {
                AddLog("Invalid value for K. Must be between 1 and 20.");
            }
        }
        catch (Exception)
        {
            AddLog("Invalid input for K.");
        }
    }

    private void SaveLogs_Click(object sender, RoutedEventArgs e)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Title = "Save Logs",
            InitialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "testing", "test"),
            DefaultExt = ".json",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                string filePath = saveFileDialog.FileName;
                string json = JsonSerializer.Serialize(_logs, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
                AddLog($"Logs saved to {filePath}");
            }
            catch (Exception ex)
            {
                AddLog($"Error saving logs: {ex.Message}");
            }
        }
    }

    private void LoadLogs_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Title = "Load Logs",
            InitialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "testing", "test"),
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                string filePath = openFileDialog.FileName;
                string jsonContent = File.ReadAllText(filePath);
                _logs = JsonSerializer.Deserialize<List<LogEntry>>(jsonContent);

                // Очищаємо поточний лог і відображаємо завантажені записи
                LogTextBox.Text = "";
                foreach (var entry in _logs)
                {
                    LogTextBox.AppendText($"[{entry.Timestamp}] {entry.Message}\n");
                }

                AddLog($"Logs loaded from {filePath}");
            }
            catch (Exception ex)
            {
                AddLog($"Error loading logs: {ex.Message}");
            }
        }
    }

    private void AddLog(string message)
    {
        var timeStamp = DateTime.Now;
        var logEntry = new LogEntry(timeStamp, message);
        _logs.Add(logEntry);

        LogTextBox.AppendText($"[{timeStamp:yyyy-MM-dd HH:mm:ss}] {message}\n");
        LogTextBox.ScrollToEnd();
    }
}