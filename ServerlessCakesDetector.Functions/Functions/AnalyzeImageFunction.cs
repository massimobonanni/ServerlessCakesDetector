using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using ServerlessCakesDetector.Core.Interfaces;
using Microsoft.WindowsAzure.Storage.Blob;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using ServerlessCakesDetector.Functions.Responses;

namespace ServerlessCakesDetectors.Functions.Functions
{
    public  class AnalyzeImageFunction
    {
		private readonly ILogger<AnalyzeImageFunction> logger;
		private readonly IImageAnalyzer imageAnalyzer;
		private readonly IConfiguration configuration;
		private readonly IImageProcessor imageProcessor;

		public AnalyzeImageFunction(IImageAnalyzer imageAnalyzer, IConfiguration configuration,
			IImageProcessor imageProcessor,ILogger<AnalyzeImageFunction> log)
		{
			logger = log;
			this.imageAnalyzer = imageAnalyzer;
			this.imageProcessor = imageProcessor;
			this.configuration = configuration;
		}

		[FunctionName(nameof(AnalyzeImageFunction))]
        public  async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function,  "post", "analyze")] HttpRequest req,
			[Blob("%DestinationContainer%", FileAccess.ReadWrite, Connection = "StorageConnectionString")] CloudBlobContainer destinationContainer,
			[EventGrid(TopicEndpointUri = "TopicEndpoint", TopicKeySetting = "TopicKey")] IAsyncCollector<EventGridEvent> eventCollector)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

           
            return new ObjectResult(new AnalyzeImageFromStreamResponse());
        }
    }
}
