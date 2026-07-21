namespace Orleans.Grains.States;

[GenerateSerializer]
public record PaymentState
{
    [Id(0)]
    public int RetryCount { get; set; } = 0;

    [Id(1)]
    public int MaxRetries { get; set; } = 3;
}
