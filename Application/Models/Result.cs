namespace Application.Models;

public class Result(int id, int userId, decimal totalInvestment, decimal projectedValue)
{
    public int Id { get; } = id;
    public int UserId { get; } = userId;
    public decimal TotalInvestment { get; } = totalInvestment;
    public decimal ProjectedValue { get; } = projectedValue;
}