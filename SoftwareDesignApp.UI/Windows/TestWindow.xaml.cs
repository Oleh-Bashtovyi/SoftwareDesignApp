using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace SoftwareDesignApp.UI.Windows;

public partial class TestWindow : Window
{
    private TestManager testManager;
    private Tester tester;
    private string codePath;
    private int kValue = 10; // значення K (за замовчуванням)
    private List<LogEntry> logs = new List<LogEntry>();

    public TestWindow()
    {
        InitializeComponent();

        // Робимо вікно модальним
        this.Topmost = true;
        this.Focus();

        // Ініціалізуємо класи для роботи з тестами
        testManager = new TestManager(); // використовує папку testing/test за замовчуванням
        tester = new Tester();
    }

    private void LoadTests_Click(object sender, RoutedEventArgs e)
    {
        List<string> foundTests = testManager.FindExistingTests();
        if (foundTests.Count > 0)
        {
            testManager.LoadTestsFromJson(foundTests[0]);
            AddLog($"Loaded tests from: {foundTests[0]}");
            StatusLabel.Content = $"Tests loaded: {Path.GetFileName(foundTests[0])}";
        }
        else
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select JSON test file",
                InitialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "testing", "test"),
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                testManager.LoadTestsFromJson(filePath);
                AddLog($"Loaded tests from: {filePath}");
                StatusLabel.Content = $"Tests loaded: {Path.GetFileName(filePath)}";
            }
        }
    }

    private void LoadCode_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Title = "Select code file to test",
            Filter = "Python files (*.py;*.txt)|*.py;*.txt|All files (*.*)|*.*",
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

    private void RunTests_Click(object sender, RoutedEventArgs e)
    {
        if (!testManager.TestsLoaded())
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
            var testResults = tester.RunTests(codePath, testManager.GetTests(), kValue);
            double coveragePercent = testResults.CoveragePercent;

            int passedCount = 0;
            int totalVariants = 0;

            foreach (var testData in testResults.TestData)
            {
                foreach (var variantInfo in testData.Log)
                {
                    totalVariants++;
                    if (variantInfo.Status == "OK")
                    {
                        passedCount++;
                    }
                }
            }

            string summary = $"Test Summary:\n" +
                $"Passed (OK): {passedCount} / {totalVariants}\n" +
                $"Coverage for <= {kValue} steps: {coveragePercent:F2}%";
            AddLog(summary);

            for (int i = 0; i < testResults.TestData.Count; i++)
            {
                var testData = testResults.TestData[i];
                AddLog($"Test #{i + 1}: executed {testData.VariantsExecuted} variants, " +
                       $"correct: {testData.CorrectVariants}, " +
                       $"coverage: {testData.CoveragePercent:F2}%");

                foreach (var variantInfo in testData.Log)
                {
                    int varNum = variantInfo.Variant;
                    string varStatus = variantInfo.Status;
                    string varOutput = variantInfo.Output.Trim();
                    AddLog($"  Variant #{varNum}: {varStatus} (output: {varOutput})");
                }
            }
        }
        catch (Exception ex)
        {
            AddLog($"Error during testing: {ex.Message}");
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


public class TestManager
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

// Класи для результатів тестування
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
}