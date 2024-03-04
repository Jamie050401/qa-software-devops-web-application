namespace Application.Common;

using System;

public class Error(int errorCode)
{
    public int ErrorCode { get; } = errorCode;
    public required string ErrorMessage { get; init; }
    public Exception? Exception { get; init; }
}