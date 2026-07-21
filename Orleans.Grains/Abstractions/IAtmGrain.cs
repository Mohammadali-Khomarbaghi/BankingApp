namespace Orleans.Grains.Abstractions;

public interface IAtmGrain : IGrainWithGuidKey
{
    [Transaction(TransactionOption.Create)]
    public Task InitialiseAsync(decimal openingBalance);

    [Transaction(TransactionOption.Create)]
    Task<decimal> GetBalanceAsync();

    [Transaction(TransactionOption.CreateOrJoin)]
    public Task WithdrawAsync(Guid CheckingAccountId , decimal amount);
}
