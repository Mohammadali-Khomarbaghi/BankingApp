namespace Orleans.Grains.States;

[GenerateSerializer]
public record RecurringPayment
{
    [Id(0)]
    public Guid PaymentId { get; set; }
    [Id(1)]
    public decimal PaymentAmount { get; set; }
    [Id(2)]
    public int OccursEveryMinutes { get; set; }
}
