namespace SoftwareDesignApp.Core;

public class TestCase
{
    public string ExpectedResult { get; set; }
}

public class TestingResult
{
    public HashSet<string> UniqueOutputs { get; set; }
    public List<TestCaseResult> TestCaseResults { get; set; }
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
    public TestingResult RunTests(string code, List<TestCase> testCases, int attempts)
    {
        var uniqueOutputs = new HashSet<string>();
        var results = new List<TestCaseResult>();
        var codeCompiler = new CompilationService();

        foreach (var testCase in testCases)
        {
            var expected = CleanOutput(testCase.ExpectedResult);
            var testCaseResult = new TestCaseResult
            {
                Code = code,
                ExpectedResult = expected
            };

            for (int i = 0; i < attempts; i++)
            {
                var compiled = codeCompiler.CompileAndExecuteFromString(code);
                var cleaned = FilterExecutionOutput(compiled.Output);

                if (cleaned == expected)
                    testCaseResult.SuccessCount++;

                uniqueOutputs.Add(cleaned);
                testCaseResult.Attempts++;
            }

            results.Add(testCaseResult);
        }

        return new TestingResult
        {
            UniqueOutputs = uniqueOutputs,
            TestCaseResults = results
        };
    }

    private string CleanOutput(string output) =>
        output.Replace("\r", "").Trim();

    private string FilterExecutionOutput(string rawOutput)
    {
        var lines = CleanOutput(rawOutput).Split('\n');
        if (lines.Length >= 3)
        {
            return string.Join("\n", lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).Take(lines.Length - 3));
        }
        return string.Join("\n", lines);
    }
}