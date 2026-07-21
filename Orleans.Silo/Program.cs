using Microsoft.Extensions.Hosting;
using Orleans.Configuration;

await Host.CreateDefaultBuilder(args)
    .UseOrleans(siloBuilder =>
    {
        siloBuilder.UseAzureStorageClustering(configureOptions: option =>
        {
            option.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true;");
        });

        siloBuilder.Configure<ClusterOptions>(option =>
        {
            option.ClusterId = "Orleans.Cluster";
            option.ServiceId = "Olreans.Service";
        });

        siloBuilder.AddAzureTableGrainStorage("OrleansTableStorage", options =>
        {
            options.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true;");
        });

        siloBuilder.AddAzureBlobGrainStorage("OrleansBlobStorage", options =>
        {
            options.BlobServiceClient = new Azure.Storage.Blobs.BlobServiceClient("UseDevelopmentStorage=true;");
        });

        siloBuilder.UseAzureTableReminderService(configureOptions: option =>
        {
            option.Configure(o => o.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true;"));
        });

        siloBuilder.AddAzureTableTransactionalStateStorageAsDefault(options =>
        {
            options.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true;");
        });

        siloBuilder.UseTransactions();
        //siloBuilder.Configure<GrainCollectionOptions>(options =>
        //{
        //    options.CollectionQuantum = TimeSpan.FromSeconds(20);
        //    options.CollectionAge = TimeSpan.FromSeconds(20);
        //});
        
    }).RunConsoleAsync();