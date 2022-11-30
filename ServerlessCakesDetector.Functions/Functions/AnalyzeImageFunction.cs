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
using ServerlessCakesDetector.Functions;
using System.Reflection.Metadata;
using YamlDotNet.Core.Tokens;
using System.Collections.Generic;
using ServerlessCakesDetector.Core.Models;
using System.Threading;
using System.Linq;

namespace ServerlessCakesDetectors.Functions.Functions
{
	public class AnalyzeImageFunction
	{
		private readonly ILogger<AnalyzeImageFunction> logger;
		private readonly IImageAnalyzer imageAnalyzer;
		private readonly IConfiguration configuration;
		private readonly IImageProcessor imageProcessor;
		private readonly IStorageService storageService;

		public AnalyzeImageFunction(IImageAnalyzer imageAnalyzer, IConfiguration configuration,
			IImageProcessor imageProcessor, IStorageService storageService,
			ILogger<AnalyzeImageFunction> log)
		{
			logger = log;
			this.imageAnalyzer = imageAnalyzer;
			this.imageProcessor = imageProcessor;
			this.storageService = storageService;
			this.configuration = configuration;
		}

		[FunctionName(nameof(AnalyzeImageFunction))]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "post", Route = "analyze")] HttpRequest req,
			[EventGrid(TopicEndpointUri = "TopicEndpoint", TopicKeySetting = "TopicKey")] IAsyncCollector<EventGridEvent> eventCollector)
		{
			if (!req.IsValid())
				return new BadRequestResult();

			var file = req.Form.Files[0];
			var operationContext = OperationContext.CreateContext(file);

			// Upload original image on storage account
			await file.UploadToStorageAsync(storageService, operationContext.BlobName);

			// Analyze image
			var imageAnalysisResult = await file.AnalyzeAsync(imageAnalyzer);

			//Create response DTO
			var response = await CreateResponseAsync(operationContext, imageAnalysisResult, file, default);

			if (this.configuration.IsEventGridConfigured())
			{
				// Send event using Event Grid Custom Topic
				var @event = new EventGridEvent(
					  subject: operationContext.BlobName,
					  eventType: "ImageAnalyzed",
					  dataVersion: "1.0",
					  data: response);

				await eventCollector.AddAsync(@event);
			}
			return new OkObjectResult(response);
		}


		private async Task<AnalyzeImageFromStreamResponse> CreateResponseAsync(OperationContext operationContext,
			ImageAnalyzerResult imageAnalysisResult, IFormFile file, CancellationToken cancellationToken)
		{
			var acceptedTags = this.configuration["AcceptedTags"]
				.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				.Select(s => s.ToLower());

			//Create response DTO
			var response = new AnalyzeImageFromStreamResponse()
			{
				OperationId = operationContext.OperationId,
				OriginalFileName = operationContext.OriginalFileName,
				FileName = operationContext.BlobName,
				AnalysisResult = imageAnalysisResult,
				ObjectsBlobs = new List<ObjectBlob>()
			};

			var resultBlobName = operationContext.GenerateResultFileName();

			await this.storageService.SerializeObjectToBlobAsync(response, resultBlobName, cancellationToken);

			if (imageAnalysisResult.Objects.Count > 0)
			{           // Elaborate faces
				for (int i = 0; i < imageAnalysisResult.Objects.Count && !cancellationToken.IsCancellationRequested; i++)
				{
					var @object = imageAnalysisResult.Objects[i];
					if (acceptedTags.Contains(@object.Tag.ToLower()))
					{
						var objectImageBlobName = operationContext.GenerateObjectImageFileName(i, @object);
						using (var sourceStream = file.OpenReadStream())
						{
							using (var imageStream = await imageProcessor.CropImageAsync(sourceStream, @object.Rectangle, cancellationToken))
							{
								await storageService.UploadToStorageAsync(imageStream, objectImageBlobName, cancellationToken);
							}
						}
						response.ObjectsBlobs.Add(new ObjectBlob()
						{
							BlobUrl = await storageService.GetUrnAsync(objectImageBlobName, cancellationToken),
							ObjectId = @object.Id
						});
					}
				}
			}

			return response;
		}
	}
}
