using Orleans.Grains.Abstractions;
using Orleans.Grains.States;

namespace Orleans.Grains.Grains;

public class AtmGrain : Grain, IAtmGrain
{

    private readonly IPersistentState<AtmState> _atmState;

    public AtmGrain(
        [PersistentState("atm", "OrleansTableStorage")] IPersistentState<AtmState> state)
    {
        _atmState = state;
    }

    public async Task InitialiseAsync(decimal openingBalance)
    {
        _atmState.State.Id = this.GetPrimaryKey();
        _atmState.State.Balance = openingBalance;

        await _atmState.WriteStateAsync();
    }

    public async Task WithdrawAsync(Guid CheckingAccountId, decimal amount)
    {
        await GrainFactory
             .GetGrain<ICheckingAccountGrain>(CheckingAccountId)
             .DebitAsync(amount);

        var currentAtmBalance = _atmState.State.Balance;
        var updatedBalance = currentAtmBalance - amount;
        _atmState.State.Balance = updatedBalance;

        await _atmState.WriteStateAsync();
    }
}
