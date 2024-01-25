namespace Application.Models;

public class Fund(int id, string name, decimal growthRate, decimal charge)
{
    public int Id { get; } = id;
    public string Name { get; } = name;
    public decimal GrowthRate { get; } = growthRate;
    public decimal Charge { get; } = charge;
}