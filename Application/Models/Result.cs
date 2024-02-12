namespace Application.Models;

public class Result
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public decimal TotalInvestment { get; init; }
    public decimal ProjectedValue { get; init; }
}