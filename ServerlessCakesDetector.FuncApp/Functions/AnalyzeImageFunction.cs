using Azure.Messaging.EventGrid;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServerlessCakesDetector.Core.Interfaces;
using ServerlessCakesDetector.Core.Models;
using ServerlessCakesDetector.FuncApp.Responses;

namespace ServerlessCakesDetector.FuncApp.Functions
{
    public class AnalyzeImageFunction
    {
        private readonly ILogger<AnalyzeImageFunction> logger;
        private readonly IImageAnalyzer imageAnalyzer;
        private readonly IConfiguration configuration;
        private readonly IImageProcessor imageProcessor;
        private readonly IStorageService storageService;
        private readonly EventGridPublisherClient eventClient;

        public AnalyzeImageFunction(IImageAnalyzer imageAnalyzer, IConfiguration configuration,
            IImageProcessor imageProcessor, IStorageService storageService,
            IAzureClientFactory<EventGridPublisherClient> eventClientFactory,
            ILogger<AnalyzeImageFunction> log)
        {
            logger = log;
            this.imageAnalyzer = imageAnalyzer;
            this.imageProcessor = imageProcessor;
            this.storageService = storageService;
            this.configuration = configuration;
            eventClient = eventClientFactory.CreateClient(Constants.EventGridClientName);
        }

        [Function(nameof(AnalyzeImageFunction))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "analyze")] HttpRequest req)
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

            // Send event using Event Grid Custom Topic
            var @event = new EventGridEvent(
                  subject: operationContext.BlobName,
                  eventType: "ImageAnalyzed",
                  dataVersion: "1.0",
                  data: response);

            await eventClient.SendEventAsync(@event);

            return new OkObjectResult(response);
        }


        private async Task<AnalyzeImageFromStreamResponse> CreateResponseAsync(OperationContext operationContext,
            ImageAnalyzerResult imageAnalysisResult, IFormFile file, CancellationToken cancellationToken)
        {
            var acceptedTags = configuration["AcceptedTags"]
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

            if (imageAnalysisResult.Objects.Count > 0)
            {
                // Elaborate faces
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

            var resultBlobName = operationContext.GenerateResultFileName();

            await storageService.SerializeObjectToBlobAsync(response, resultBlobName, cancellationToken);

            return response;
        }
    }
}
