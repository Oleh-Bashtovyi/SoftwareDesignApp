using Microsoft.Win32;
using SoftwareDesignApp.Core;
using SoftwareDesignApp.UI.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;

namespace SoftwareDesignApp.UI.Windows
{
    public partial class TestWindow : Window, INotifyPropertyChanged
    {
        private readonly TestManager _testManager = new();
        private int _kValue = 10;
        private string _testableCode;

        public ObservableCollection<LogViewModel> Logs { get; } = new();
        public ObservableCollection<TestCaseViewModel> TestCases { get; } = new();

        public string TestableCode
        {
            get => _testableCode;
            set => SetField(ref _testableCode, value);
        }

        public TestWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void LoadTests_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    //InitialDirectory = Directory.GetCurrentDirectory(),
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
                        TestCases.Clear();
                        var counter = 1;
                        foreach (var testCase in loadedTestCases)
                        {
                            TestCases.Add(new TestCaseViewModel($"Test case {counter}", testCase.ExpectedResult));
                            counter++;
                        }

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
            if (!TestCases.Any())
            {
                AddLog("No tests loaded. Please load tests first.");
                return;
            }
            if (string.IsNullOrEmpty(TestableCode))
            {
                AddLog("No code file loaded. Please load a code file to test.");
                return;
            }

            AddLog("Starting tests...");
            try
            {
                var testCases = TestCases.Select(tc => tc.ToCoreTestCase()).ToList();
                var result = _testManager.RunTests(TestableCode, testCases, _kValue);

                var totalAttempts = result.TestCaseResults.Sum(x => x.Attempts);
                var totalSuccess = result.TestCaseResults.Sum(x => x.SuccessCount);
                var successfulTestCasesCount = result.TestCaseResults.Count(tc => tc.SuccessCount > 0);
                var testCasesCount = testCases.Count;
                var testCasesSuccessRate = (double)successfulTestCasesCount / testCasesCount * 100;
                var generalResults = $"Unique outputs: {result.UniqueOutputs.Count}\n" +
                                     $"Total test cases: {testCasesCount}\n" +
                                     $"Total attempts: {totalAttempts}\n" +
                                     $"Total success: {totalSuccess}\n" +
                                     $"Number of successful test cases: {successfulTestCasesCount}\n" +
                                     $"Test cases success rate: {successfulTestCasesCount}/{testCasesCount} ({testCasesSuccessRate:#.##}%)";
                AddLog(generalResults);

                var counter = 1;
                foreach (var testCaseResult in result.TestCaseResults)
                {
                    var attempts = testCaseResult.Attempts;
                    var successCount = testCaseResult.SuccessCount;
                    var successRate = (double)successCount / attempts * 100;

                    var testCaseResultMessage = $"TEST CASE {counter}\n" +
                                                $"Attempts count: {attempts}\n" +
                                                $"Success count: {successCount}\n" +
                                                $"Success rate: {successCount}/{attempts} ({successRate:#.##}%)\n" +
                                                $"EXPECTED RESULT:\n" +
                                                $"{testCaseResult.ExpectedResult}";
                    AddLog(testCaseResultMessage);
                    counter++;
                }

                counter = 1;
                foreach (var output in result.UniqueOutputs)
                {
                    var uniqueOutputMessage = $"UNIQUE OUTPUT {counter}\n" +
                                               $"{output}";
                    AddLog(uniqueOutputMessage);
                    counter++;
                }
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
                //InitialDirectory = Directory.GetCurrentDirectory()
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var codePath = openFileDialog.FileName;
                TestableCode = File.ReadAllText(codePath);
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
                //InitialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "testing", "test"),
                DefaultExt = ".json",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = saveFileDialog.FileName;
                    string json = JsonSerializer.Serialize(Logs, new JsonSerializerOptions { WriteIndented = true });
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
                //InitialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "testing", "test"),
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = openFileDialog.FileName;
                    string jsonContent = File.ReadAllText(filePath);
                    var loadedLogs = JsonSerializer.Deserialize<List<LogViewModel>>(jsonContent);

                    if (loadedLogs != null)
                    {
                        Logs.Clear();
                        foreach (var log in loadedLogs)
                        {
                            Logs.Add(new LogViewModel(log.Message, log.Timestamp));
                        }

                        AddLog($"Logs loaded from {filePath}");
                    }
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
            Logs.Add(new LogViewModel(message, timeStamp));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            Logs.Clear();
        }
    }
}