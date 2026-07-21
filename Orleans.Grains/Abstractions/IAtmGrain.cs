namespace Orleans.Grains.Abstractions;

public interface IAtmGrain : IGrainWithGuidKey
{
    public Task InitialiseAsync(decimal openingBalance);
    public Task WithdrawAsync(Guid CheckingAccountId , decimal amount);
}
