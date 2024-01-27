namespace Application.Common;

public class Error(int errorCode)
{
    public int ErrorCode { get; } = errorCode;
    public string ErrorMessage { get; set; } = "";
    public Exception? Exception { get; set; }
}