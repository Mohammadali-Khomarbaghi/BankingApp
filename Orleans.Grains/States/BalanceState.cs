namespace Orleans.Grains.States;

[GenerateSerializer]
public record BalanceState
{
    [Id(0)]
    public decimal Balance { get; set; }
}
