namespace Orleans.Grains.Abstractions;

public interface ICheckingAccountGrain : IGrainWithGuidKey
{
    Task InitialiseAsync(decimal openingBalance);
    Task<decimal> GetBalanceAsync();
    Task DebitAsync(decimal amount);
    Task CreditAsync(decimal amount);
    Task AddReccuringPaymentAsync(Guid id, decimal amount,int reccursEveryMinutes);
}
