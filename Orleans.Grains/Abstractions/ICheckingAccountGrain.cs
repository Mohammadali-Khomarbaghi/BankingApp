namespace Orleans.Grains.Abstractions;

public interface ICheckingAccountGrain : IGrainWithGuidKey
{
    [Transaction(TransactionOption.Create)]
    Task InitialiseAsync(decimal openingBalance);

    [Transaction(TransactionOption.Create)]
    Task<decimal> GetBalanceAsync();

    [Transaction(TransactionOption.CreateOrJoin)]
    Task DebitAsync(decimal amount);

    [Transaction(TransactionOption.CreateOrJoin)]
    Task CreditAsync(decimal amount);

    Task AddReccuringPaymentAsync(Guid id, decimal amount,int reccursEveryMinutes);
}
