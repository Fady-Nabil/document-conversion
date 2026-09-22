namespace Shared.Exceptions;

public class BusinessException(string code, string message, Exception? innerException = null) 
    : Exception(message, innerException)
{
    public string Code { get; } = code;
}
