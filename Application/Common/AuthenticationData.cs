namespace Application.Common;

public class AuthenticationData
{
    public required string Email { get; init; }
    public required string Token { get; set; }
    public required string Source { get; init; }
    public DateTime Timestamp { get; init; }
    public DateTimeOffset Expires { get; init; }
}