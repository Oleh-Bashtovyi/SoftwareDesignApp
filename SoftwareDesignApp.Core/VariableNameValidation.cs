using System.Text.RegularExpressions;

namespace SoftwareDesignApp.Core;

public enum ValidationErrorType
{
    None,
    EmptyName,
    InvalidFirstChar,
    InvalidChars,
    ReservedKeyword
}

public class VariableValidationResult(string? variable, ValidationErrorType validationErrorType, string? errorMessage = null)
{
    public bool IsValid => ValidationErrorType == ValidationErrorType.None;
    public ValidationErrorType ValidationErrorType { get; } = validationErrorType;
    public string? Variable { get; } = variable;
    public string? ErrorMessage { get; } = errorMessage;
}

public class VariableValidationException(VariableValidationResult validationResult) : Exception(validationResult.ErrorMessage)
{
    public VariableValidationResult ValidationResult { get; } = validationResult;

    public static void ThrowIfInvalid(string? variableName)
    {
        var validationResult = VariableNameValidation.Validate(variableName);

        if (!validationResult.IsValid)
        {
            throw new VariableValidationException(validationResult);
        }
    }
}



public static class VariableNameValidation
{
    // Зарезервовані словва, якими не можна назвати змінну
    private static readonly string[] Keywords = {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch",
        "char", "checked", "class", "const", "continue", "decimal", "default",
        "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
        "false", "finally", "fixed", "float", "for", "foreach", "goto", "if",
        "implicit", "in", "int", "interface", "internal", "is", "lock", "long",
        "namespace", "new", "null", "object", "operator", "out", "override",
        "params", "private", "protected", "public", "readonly", "ref", "return",
        "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string",
        "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong",
        "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
    };

    /*
    В C# є чіткі правила щодо найменування змінних. Ось основні обмеження щодо першого символу назви змінної:
    - Змінна не може починатися з цифри
    - Змінна не може починатися зі спеціальних символів (окрім знаку підкреслення _)
    - Змінна не може починатися з пробілу чи інших пропусків
 */



    // Метод для перевірки імені змінної з поверненням типу помилки
    public static VariableValidationResult Validate(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return new VariableValidationResult(name, ValidationErrorType.EmptyName, "Variable name can not be empty!");

        // Перевірка першого символу (повинен бути літера або _)
        if (!char.IsLetter(name[0]) && name[0] != '_')
            return new VariableValidationResult(name, ValidationErrorType.InvalidFirstChar, "Variable must start with letter or _!");

        // Перевірка всіх символів (літери, цифри, _)
        Regex validCharsRegex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");
        if (!validCharsRegex.IsMatch(name))
            return new VariableValidationResult(name, ValidationErrorType.InvalidChars, "Variable can only contain letters, digits, and underscore (_).");

        // Перевірка чи не є зарезервованим словом
        if (Array.Exists(Keywords, keyword => keyword == name))
            return new VariableValidationResult(name, ValidationErrorType.ReservedKeyword, "Variable name cannot be a C# reserved keyword.");

        return new VariableValidationResult(name, ValidationErrorType.None);
    }
}