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
});

var app = builder.Build();

app.MapGet("checkingaccount/{checkingAccountId}/balance",
    async (IClusterClient clusterclient, Guid checkingAccountId) =>
    {
        var checkingAccountGrain = clusterclient.GetGrain<ICheckingAccountGrain>(checkingAccountId);
        var balance = checkingAccountGrain.GetBalanceAsync();
        return TypedResults.Ok(balance); 

    });

app.MapPost("checkingaccount", async (
    IClusterClient clusterClient,
    CreateCheckingAccount createAccount) =>
{
    var checkingAccountId = Guid.NewGuid();
    var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);
    await checkingAccountGrain.InitialiseAsync(createAccount.OpeningBalance);
    return TypedResults.Created($"checkingaccount/{checkingAccountId}");
});

app.MapPost("checkingaccount/{checkingAccountId}/credit", async (
    Credit credit,
    Guid checkingAccountId,
    IClusterClient clusterClient) =>
{
    var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);
    await checkingAccountGrain.CreditAsync(credit.Amount);
    return TypedResults.NoContent();
});

app.MapPost("checkingaccount/{checkingAccountId}/debit", async (
    Debit debit,
    Guid checkingAccountId,
    IClusterClient clusterClient) =>
{
    var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);
    await checkingAccountGrain.DebitAsync(debit.Amount);
    return TypedResults.NoContent();
});

app.MapPost("checkingaccount/{checkingAccountId}/recurringPayment", async (
    CreateRecurringPayment createRecurringPayment,
    Guid checkingAccountId,
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
    IClusterClient clusterClient) =>
{
    var atmId = Guid.NewGuid();
    var atmGrain = clusterClient.GetGrain<IAtmGrain>(atmId);
    await atmGrain.InitialiseAsync(createAtm.OpeningBalance);
    return TypedResults.Created($"atm/{atmId}");
});


app.MapPost("atm/{atmId}/withdraw", async (
    AtmWithdraw atmWithdraw,
    Guid atmId,
    IClusterClient clusterClient) =>
{
    var atmGrain = clusterClient.GetGrain<IAtmGrain>(atmId);
    await atmGrain.WithdrawAsync(atmWithdraw.CheckingAccountId, atmWithdraw.Amount);
    return TypedResults.NoContent();    
});


app.Run();
