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

		private ImageAnalyzerResult ElaborateImagePrediction(ImagePrediction imagePrediction,
			ImageAnalyzerConfiguration config)
		{
			var imageResult = new ImageAnalyzerResult() { 
				Objects= new List<ObjectInfo>()
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

		public async Task<ImageAnalyzerResult> AnalyzeImageAsync(Stream imageStream, CancellationToken cancellationToken = default)
		{
			var config = ImageAnalyzerConfiguration.Load(configuration);

			try
			{
				var visionClient = CreateVisionClient(config);
				var imagePrediction = await visionClient.ClassifyImageAsync(
							new Guid(config.ProjectId),	config.ModelName, 
							imageStream, null, cancellationToken);
				return ElaborateImagePrediction(imagePrediction, config);
			}
			catch (Exception ex)
			{
				this.logger.LogError(ex, "Error during custom vision prediction service");
				throw;
				//return ImageAnalyzerResult.Empty;
			}
		}

		public async Task<ImageAnalyzerResult> AnalyzeImageAsync(string imageUrl, CancellationToken cancellationToken = default)
		{
			var config = ImageAnalyzerConfiguration.Load(configuration);

			try
			{
				var visionClient = CreateVisionClient(config);
				var imagePrediction = await visionClient.ClassifyImageUrlAsync(
							new Guid(config.ProjectId),config.ModelName, 
							new ImageUrl(imageUrl), null, cancellationToken);
				return ElaborateImagePrediction(imagePrediction, config);
			}
			catch (Exception ex)
			{
				this.logger.LogError(ex, "Error during custom vision prediction service");
				throw;
				//return ImageAnalyzerResult.Empty;
			}
		}
	}
}
