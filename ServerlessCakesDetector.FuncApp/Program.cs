using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServerlessCakesDetector.Cognitive.Services;
using ServerlessCakesDetector.Core.Interfaces;
using ServerlessCakesDetector.FuncApp;
using ServerlessCakesDetector.FuncApp.Services;
using ServerlessCakesDetector.ImageProcessing;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddLogging();
builder.Services.AddApplicationInsightsTelemetryWorkerService();
builder.Services.ConfigureFunctionsApplicationInsights();
builder.Services.AddAzureClients(b =>
{
    b.AddBlobServiceClient(builder.Configuration["StorageConnectionString"])
         .WithName(Constants.BlobClientName);
    b.AddEventGridPublisherClient(new Uri(builder.Configuration["TopicEndpoint"]),
                                new AzureKeyCredential(builder.Configuration["TopicKey"]))
         .WithName(Constants.EventGridClientName);
});

builder.Services.AddScoped<IImageAnalyzer, CustomVisionImageAnalyzer>();
builder.Services.AddScoped<IImageProcessor, ImageProcessor>();
builder.Services.AddScoped<IStorageService, BlobStorageService>();

builder.Build().Run();
