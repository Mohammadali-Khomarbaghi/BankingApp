namespace Orleans.Grains.States;

[GenerateSerializer]
public record CheckingAccountState
{
    [Id(0)]
    public Guid AccountId { get; set; }
    [Id(1)]
    public DateTime OpenedAtUtc { get; set; }
    [Id(2)]
    public string AccountType { get; set; } = string.Empty;
    [Id(3)]
    public List<RecurringPayment> RecurringPayments { get; set; } = new();
}
