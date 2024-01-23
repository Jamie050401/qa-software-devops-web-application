namespace Application.Models;

public class Fund
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal GrowthRate { get; set; }
    public decimal Charge { get; set; }
}