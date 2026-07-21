using Orleans.Concurrency;
using Orleans.Grains.Abstractions;
using Orleans.Grains.States;
using Orleans.Transactions.Abstractions;

namespace Orleans.Grains.Grains;

[Reentrant]
public class AtmGrain : Grain, IAtmGrain
{

    private readonly ITransactionalState<AtmState> _atmTransactionalState;

    public AtmGrain(
        [TransactionalState("atm")] ITransactionalState<AtmState> atmTransactionalState)
    {
        _atmTransactionalState = atmTransactionalState;
    }

    public async Task InitialiseAsync(decimal openingBalance)
    {
        await _atmTransactionalState.PerformUpdate(state =>
        {
            state.Id = this.GetPrimaryKey();
            state.Balance = openingBalance;
        });
    }

    public async Task<decimal> GetBalanceAsync()
    {
        return await _atmTransactionalState.PerformRead(state => state.Balance);
    }

    public async Task WithdrawAsync(Guid CheckingAccountId, decimal amount)
    {
        await _atmTransactionalState.PerformUpdate(state =>
        {
            var currentAtmBalance = state.Balance;
            var updatedBalance = currentAtmBalance - amount;
            state.Balance = updatedBalance;
        });
    }
}
