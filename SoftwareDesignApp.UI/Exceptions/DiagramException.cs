using SoftwareDesignApp.UI.Enums;

namespace SoftwareDesignApp.UI.Exceptions;

public class DiagramException(string diagramName, DiagramErrorCode errorCode, string message) : Exception(message)
{
    public string DiagramName { get; } = diagramName;
    public DiagramErrorCode ErrorCode { get; } = errorCode;
}