using SoftwareDesignApp.Core;

namespace SoftwareDesignApp.UI.ViewModels;

public class TestCaseViewModel(string testCaseName, string expectedResult) : BaseViewModel
{
    private string _testCaseName = testCaseName;
    private string _expectedResult = expectedResult;

    public string TestCaseName
    {
        get => _testCaseName;
        set => SetField(ref _testCaseName, value);
    }

    public string ExpectedResult
    {
        get => _expectedResult;
        set => SetField(ref _expectedResult, value);
    }

    public TestCase ToCoreTestCase()
    {
        return new TestCase()
        {
            ExpectedResult = ExpectedResult
        };
    }
}