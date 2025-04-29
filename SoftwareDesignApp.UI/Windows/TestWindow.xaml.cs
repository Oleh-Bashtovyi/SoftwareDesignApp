using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using SoftwareDesignApp.Core;

namespace SoftwareDesignApp.UI.Windows;

public partial class TestWindow : Window
{
    private TestManager testManager;
    private string codePath;
    private string testPath;
    private int kValue = 10; // значення K (за замовчуванням)
    private List<TestCase> testCases = new List<TestCase>();
    private List<LogEntry> logs = new List<LogEntry>();

    public TestWindow()
    {
        InitializeComponent();
        testManager = new TestManager(); 
    }

    private void LoadTests_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select JSON test file",
                InitialDirectory = Directory.GetCurrentDirectory(),
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                var content = File.ReadAllText(filePath);
                testCases = JsonSerializer.Deserialize<List<TestCase>>(content);
                if (testCases != null)
                {
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
        if (!testCases.Any())
        {
            AddLog("No tests loaded. Please load tests first.");
            return;
        }
        if (string.IsNullOrEmpty(codePath))
        {
            AddLog("No code file loaded. Please load a code file to test.");
            return;
        }

        AddLog("Starting tests...");
        try
        {
            var code = File.ReadAllText(codePath);
            var testResults = testManager.RunTests(code, testCases, kValue);
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
                $"Coverage for <= {kValue} steps: {coveragePercent:F2}%";
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
            codePath = openFileDialog.FileName;
            AddLog($"Loaded code file: {codePath}");
            StatusLabel.Content = $"Code loaded: {Path.GetFileName(codePath)}";
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
                kValue = value;
                AddLog($"Set K to {kValue}");
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
                string json = JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true });
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
                logs = JsonSerializer.Deserialize<List<LogEntry>>(jsonContent);

                // Очищаємо поточний лог і відображаємо завантажені записи
                LogTextBox.Text = "";
                foreach (var entry in logs)
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
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        LogEntry logEntry = new LogEntry
        {
            Timestamp = timestamp,
            Message = message
        };

        logs.Add(logEntry);
        LogTextBox.AppendText($"[{timestamp}] {message}\n");
        LogTextBox.ScrollToEnd();
    }
}

public class LogEntry
{
    public string Timestamp { get; set; }
    public string Message { get; set; }
}


public class TestCase
{
    public string ExpectedResult { get; set; }
}

public class TestCaseResult
{
    public string Code { get; set; }
    public string ExpectedResult { get; set; }
    public int Attempts { get; set; }
    public int SuccessCount { get; set; }
    public double CoveragePercent => SuccessCount / (double)Attempts * 100;
}


public class TestManager
{
    public List<TestCaseResult> RunTests(string code, List<TestCase> testCases, int attempts)
    {
        var results = new List<TestCaseResult>();

        foreach (var testCase in testCases)
        {
            var testCaseResult = new TestCaseResult();
            var expectedValue = testCase.ExpectedResult.Replace("\r", "");
            var codeCompiler = new CompilationService();

            for (int i = 0; i < attempts; i++)
            {
                var compiledCode = codeCompiler.CompileAndExecuteFromString(code);
                var rawOutput = compiledCode.Output.Replace("\r", "").Trim();
                var lines = rawOutput.Split('\n');

                string cleanedOutput = rawOutput;
                if (lines.Length >= 3)
                {
                    cleanedOutput = string.Join("\n", lines.Skip(1).Where(x => !string.IsNullOrWhiteSpace(x)).Take(lines.Length - 3));
                }

                if (cleanedOutput == expectedValue)
                {
                    testCaseResult.SuccessCount++;
                }

                testCaseResult.Attempts++;
            }
            testCaseResult.Code = code;
            testCaseResult.ExpectedResult = expectedValue;
            results.Add(testCaseResult);
        }

        return results;
    }
}




/*public class TestManager
{
    public List<string> FindExistingTests()
    {
        // Тут має бути реалізація пошуку тестів
        return new List<string>();
    }

    public void LoadTestsFromJson(string filePath)
    {
        // Тут має бути реалізація завантаження тестів з JSON
    }

    public bool TestsLoaded()
    {
        // Заглушка - перевірка, чи завантажено тести
        return false;
    }

    public object GetTests()
    {
        // Заглушка - повертає завантажені тести
        return new object();
    }
}

public class Tester
{
    public TestResult RunTests(string codePath, object tests, int kValue)
    {
        // Заглушка - тут має бути виконання тестів
        return new TestResult();
    }
}

public class TestResult
{
    public List<TestData> TestData { get; set; } = new List<TestData>();
    public double CoveragePercent { get; set; }
}

public class TestData
{
    public int VariantsExecuted { get; set; }
    public int CorrectVariants { get; set; }
    public double CoveragePercent { get; set; }
    public List<VariantInfo> Log { get; set; } = new List<VariantInfo>();
}

public class VariantInfo
{
    public int Variant { get; set; }
    public string Status { get; set; }
    public string Output { get; set; }
}*/