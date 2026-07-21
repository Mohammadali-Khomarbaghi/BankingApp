using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Differencing;
using Orleans.Client.Contracts;
using Orleans.Configuration;
using Orleans.Grains.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleansClient((context, client) =>
{
    client.UseAzureStorageClustering(configureOptions: option =>
    {
        option.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true");
    });

    client.Configure<ClusterOptions>(option =>
    {
        option.ClusterId = "Orleans.Cluster";
        option.ServiceId = "Olreans.Service";
    });

    client.UseTransactions();
});

var app = builder.Build();

app.MapGet("checkingaccount/{checkingAccountId}/balance", async (
    Guid checkingAccountId,
    IClusterClient clusterclient,
    [FromServices] ITransactionClient transactionClient) =>
{
    var balance = 0m;
    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
        var checkingAccountGrain = clusterclient.GetGrain<ICheckingAccountGrain>(checkingAccountId);
        balance = await checkingAccountGrain.GetBalanceAsync();
    });
    return TypedResults.Ok(balance);
});

app.MapPost("checkingaccount", async (
    CreateCheckingAccount createAccount,
    IClusterClient clusterClient,
    [FromServices] ITransactionClient transactionClient) =>
{
    var checkingAccountId = Guid.NewGuid();
    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);
        await checkingAccountGrain.InitialiseAsync(createAccount.OpeningBalance);
    });
    return TypedResults.Created($"checkingaccount/{checkingAccountId}");
});

app.MapPost("checkingaccount/{checkingAccountId}/credit", async (
    Credit credit,
    Guid checkingAccountId,
    IClusterClient clusterClient,
    [FromServices] ITransactionClient transactionClient) =>
{
    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);
        await checkingAccountGrain.CreditAsync(credit.Amount);
    });
    return TypedResults.NoContent();
});

app.MapPost("checkingaccount/{checkingAccountId}/debit", async (
    Guid checkingAccountId,
    Debit debit,
    IClusterClient clusterClient,
    [FromServices] ITransactionClient transactionClient) =>
{
    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);
        await checkingAccountGrain.DebitAsync(debit.Amount);
    });
    return TypedResults.NoContent();
});

app.MapPost("checkingaccount/{checkingAccountId}/recurringPayment", async (
    Guid checkingAccountId,
    CreateRecurringPayment createRecurringPayment,
    IClusterClient clusterClient) =>
{
    var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);
    await checkingAccountGrain.AddReccuringPaymentAsync(
        createRecurringPayment.PaymentId,
        createRecurringPayment.PaymentAmount,
        createRecurringPayment.PaymentRecurrsEveryMinutes);
    return TypedResults.NoContent();
});

app.MapPost("atm", async (
    CreateAtm createAtm,
    IClusterClient clusterClient,
    [FromServices] ITransactionClient transactionClient) =>
{
    var atmId = Guid.NewGuid();
    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
        var atmGrain = clusterClient.GetGrain<IAtmGrain>(atmId);
        await atmGrain.InitialiseAsync(createAtm.OpeningBalance);
    });
    return TypedResults.Created($"atm/{atmId}");
});

app.MapGet("atm/{atmId}/balance", async (
    Guid atmId,
    IClusterClient clusterClient,
    [FromServices] ITransactionClient transactionClient) =>
{
    var balance = 0m;
    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
        var atmGrain = clusterClient.GetGrain<IAtmGrain>(atmId);
        balance = await atmGrain.GetBalanceAsync();
    });
    return TypedResults.Ok(balance);
});

app.MapPost("atm/{atmId}/withdraw", async (
    Guid atmId,
    AtmWithdraw atmWithdraw,
    IClusterClient clusterClient,
    [FromServices] ITransactionClient transactionClient) =>
{
    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(atmWithdraw.CheckingAccountId);
        var atmGrain = clusterClient.GetGrain<IAtmGrain>(atmId);
        await atmGrain.WithdrawAsync(atmWithdraw.CheckingAccountId, atmWithdraw.Amount);
        await checkingAccountGrain.DebitAsync(atmWithdraw.Amount);
    });
    return TypedResults.NoContent();
});


app.Run();
