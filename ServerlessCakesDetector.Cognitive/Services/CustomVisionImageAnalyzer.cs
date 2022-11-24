using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ServerlessCakesDetector.Cognitive.Configuration;
using ServerlessCakesDetector.Core.Interfaces;
using ServerlessCakesDetector.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessCakesDetector.Cognitive.Services
{
	public class CustomVisionImageAnalyzer : IImageAnalyzer
	{
		private readonly IConfiguration configuration;
		private readonly ILogger<CustomVisionImageAnalyzer> logger;

		public CustomVisionImageAnalyzer(IConfiguration configuration,
			ILoggerFactory loggerFactory)
		{
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));
			if (loggerFactory == null)
				throw new ArgumentNullException(nameof(loggerFactory));

			this.configuration = configuration;
			this.logger = loggerFactory.CreateLogger<CustomVisionImageAnalyzer>();
		}

		private CustomVisionPredictionClient CreateVisionClient(ImageAnalyzerConfiguration config)
		{
			return new CustomVisionPredictionClient(
				new ApiKeyServiceClientCredentials(config.PredictionKey))
			{
				Endpoint = config.PredictionEndpoint
			};
		}

		private ImageAnalyzerResult ElaborateImagePrediction(ImagePrediction imagePrediction, long elapsedMilliseconds,
			ImageAnalyzerConfiguration config)
		{
			var imageResult = new ImageAnalyzerResult()
			{
				ElapsedTimeInMilliseconds=elapsedMilliseconds,
				Objects = new List<ObjectInfo>()
			};

			foreach (var prediction in imagePrediction.Predictions)
			{
				if (prediction.Probability >= config.Threshold)
				{
					var @object = new ObjectInfo()
					{
						Id = Guid.NewGuid().ToString(),
						Rectangle = prediction.BoundingBox.ToImageRectangle(),
						Tag = prediction.TagName
					};
					imageResult.Objects.Add(@object);
				}
			}

			return imageResult;
		}

		private async Task<ImageAnalyzerResult> AnalyzeImageAsync(
			Func<CustomVisionPredictionClient, ImageAnalyzerConfiguration, Task<ImagePrediction>> cognitiveCallLambda,
			CancellationToken cancellationToken = default)
		{
			var config = ImageAnalyzerConfiguration.Load(configuration);

			try
			{
				var visionClient = CreateVisionClient(config);
				var stopWatch = Stopwatch.StartNew();
				var imagePrediction = await cognitiveCallLambda.Invoke(visionClient, config);
				stopWatch.Stop();
				return ElaborateImagePrediction(imagePrediction, stopWatch.ElapsedMilliseconds, config);
			}
			catch (Exception ex)
			{
				this.logger.LogError(ex, "Error during custom vision prediction service");
				throw;
				//return ImageAnalyzerResult.Empty;
			}
		}

		public async Task<ImageAnalyzerResult> AnalyzeImageAsync(Stream imageStream, CancellationToken cancellationToken = default)
		{
			return await AnalyzeImageAsync((client, config) =>
			{
				return client.DetectImageAsync(config.ProjectId, 
					config.ModelName, imageStream, null, cancellationToken);
			});
		}
			

		public async Task<ImageAnalyzerResult> AnalyzeImageAsync(string imageUrl, CancellationToken cancellationToken = default)
		{
			return await AnalyzeImageAsync((client, config) =>
			{
				return client.DetectImageUrlAsync(
							config.ProjectId, config.ModelName,
							new ImageUrl(imageUrl), null, cancellationToken);
			});
		}
	}
}
