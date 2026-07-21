namespace Orleans.Grains.States;

[GenerateSerializer]
public record class AtmState
{
    [Id(0)]
    public Guid Id { get; set; }
    [Id(1)]
    public decimal Balance { get; set; }
}
