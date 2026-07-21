using Orleans.Concurrency;
using Orleans.Grains.Abstractions;
using Orleans.Grains.States;
using Orleans.Transactions.Abstractions;

namespace Orleans.Grains.Grains;

[Reentrant]
public class CheckingAccountGrain : Grain, ICheckingAccountGrain, IRemindable
{

    private readonly ITransactionClient _transactionClient;
    private readonly ITransactionalState<BalanceState> _balanceTransactionalState;
    private readonly IPersistentState<PaymentState> _paymentState;
    private readonly IPersistentState<CheckingAccountState> _checkingAccountState;

    public CheckingAccountGrain(
        ITransactionClient transactionClient,
        [TransactionalState("balance")] ITransactionalState<BalanceState> balanceTransactionalState,
        [PersistentState("payment", "OrleansBlobStorage")] IPersistentState<PaymentState> paymentState,
        [PersistentState("checkingAccount", "OrleansBlobStorage")] IPersistentState<CheckingAccountState> checkingAccountState)
    {
        _transactionClient = transactionClient;
        _balanceTransactionalState = balanceTransactionalState;
        _paymentState = paymentState;
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

        _paymentState.State.RetryCount = 0;
        await _paymentState.WriteStateAsync();

        await this.RegisterOrUpdateReminder($"RecrringPayment:::{id}",
            TimeSpan.FromMinutes(reccursEveryMinutes),
            TimeSpan.FromMinutes(reccursEveryMinutes));
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (reminderName.StartsWith($"RecrringPayment"))
        {
            _paymentState.State.RetryCount++;
            var paymentId = Guid.Parse(reminderName.Split(":::").Last());

            if (_paymentState.State.RetryCount >= _paymentState.State.MaxRetries)
            {
                IGrainReminder reminder = await this.GetReminder($"RecrringPayment:::{paymentId}");
                if (reminder != null)
                {
                    await this.UnregisterReminder(reminder);
                }

                var FinishedrecurringPaymentIndex = _checkingAccountState.State.RecurringPayments.FindIndex(state => state.PaymentId == paymentId);
                _checkingAccountState.State.RecurringPayments.RemoveAt(FinishedrecurringPaymentIndex);
                await _checkingAccountState.WriteStateAsync();
            }
            else
            {

                var recurringPayment = _checkingAccountState.State.RecurringPayments.Single(p => p.PaymentId == paymentId);
                if (recurringPayment != null)
                {
                    Console.WriteLine($"Recurring Payment Triggered: {recurringPayment.PaymentAmount} for PaymentId: {paymentId}");
                    await _transactionClient.RunTransaction(TransactionOption.Create, async () =>
                    {
                        await DebitAsync(recurringPayment.PaymentAmount);
                    });
                }
            }
        }
    }

    public async Task CreditAsync(decimal amount)
    {
        await _balanceTransactionalState.PerformUpdate(state =>
        {
            var currentBalance = state.Balance;
            var newBalance = currentBalance + amount;
            state.Balance = newBalance;
        });
    }

    public async Task DebitAsync(decimal amount)
    {
        await _balanceTransactionalState.PerformUpdate(state =>
        {
            var currentBalance = state.Balance;
            var newBalance = currentBalance - amount;
            state.Balance = newBalance;
        });
    }

    public async Task<decimal> GetBalanceAsync()
    {
        return await _balanceTransactionalState.PerformRead(state => state.Balance);
    }

    public async Task InitialiseAsync(decimal openingBalance)
    {
        await _balanceTransactionalState.PerformUpdate(state =>
        {
            state.Balance = openingBalance;
        });
        _checkingAccountState.State.OpenedAtUtc = DateTime.UtcNow;
        _checkingAccountState.State.AccountId = this.GetPrimaryKey();
        _checkingAccountState.State.AccountType = "Default";
        await _checkingAccountState.WriteStateAsync();
    }
}
