using Orleans.Grains.Abstractions;
using Orleans.Grains.States;

namespace Orleans.Grains.Grains;

public class CheckingAccountGrain : Grain, ICheckingAccountGrain, IRemindable
{

    private readonly IPersistentState<BalanceState> _balanceState;
    private readonly IPersistentState<CheckingAccountState> _checkingAccountState;

    public CheckingAccountGrain(
        [PersistentState("balance", "OrleansTableStorage")] IPersistentState<BalanceState> balanceState,
        [PersistentState("checkingAccount", "OrleansBlobStorage")] IPersistentState<CheckingAccountState> checkingAccountState)
    {
        _balanceState = balanceState;
        _checkingAccountState = checkingAccountState;
    }

    public async Task AddReccuringPaymentAsync(Guid id, decimal amount, int reccursEveryMinutes)
    {
        _checkingAccountState.State.RecurringPayments.Add(new RecurringPayment
        {
            PaymentId = id,
            PaymentAmount = amount,
            OccursEveryMinutes = reccursEveryMinutes
        });
        await _checkingAccountState.WriteStateAsync();
        
        await this. RegisterOrUpdateReminder($"RecrringPayment:::{id}",
            TimeSpan.FromMinutes(reccursEveryMinutes),
            TimeSpan.FromMinutes(reccursEveryMinutes));
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if(reminderName.StartsWith("RecrringPayment:::"))
        {
            var paymentId = Guid.Parse(reminderName.Split(":::").Last());
            var recurringPayment = _checkingAccountState.State.RecurringPayments.Single(p => p.PaymentId == paymentId);
            if (recurringPayment != null)
            {
                Console.WriteLine($"Recurring Payment Triggered: {recurringPayment.PaymentAmount} for PaymentId: {paymentId}");
                await DebitAsync(recurringPayment.PaymentAmount);
            }
        }
    }

    public async Task CreditAsync(decimal amount)
    {
        //Timer
        //RegisterTimer(async (object obj) =>
        //{
        //    Console.WriteLine("Timer triggered");
        //    await Task.Delay(TimeSpan.FromSeconds(10));
        //    Console.WriteLine("Timer Finished");
        //}, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));

        var currentBalance = _balanceState.State.Balance;
        var newBalance = currentBalance + amount;
        _balanceState.State.Balance = newBalance;
        await _balanceState.WriteStateAsync();
    }

    public async Task DebitAsync(decimal amount)
    {
        //Console.WriteLine("-- Debit Started --");
        //await Task.Delay(TimeSpan.FromSeconds(20));
        //Console.WriteLine("-- Debit Ended --");

        var currentBalance = _balanceState.State.Balance;
        var newBalance = currentBalance - amount;
        _balanceState.State.Balance = newBalance;
        await _balanceState.WriteStateAsync();
    }

    public async Task<decimal> GetBalanceAsync()
    {
        return _balanceState.State.Balance;
    }

    public async Task InitialiseAsync(decimal openingBalance)
    {
        _checkingAccountState.State.OpenedAtUtc = DateTime.UtcNow;
        _checkingAccountState.State.AccountId = this.GetPrimaryKey();
        _checkingAccountState.State.AccountType = "Default";
        _balanceState.State.Balance = openingBalance;
        await _balanceState.WriteStateAsync();
        await _checkingAccountState.WriteStateAsync();
    }


}
