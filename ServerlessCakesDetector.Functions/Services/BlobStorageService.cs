using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessCakesDetector.Cognitive.Services;
using ServerlessCakesDetector.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ServerlessCakesDetector.Functions.Services
{
	public class BlobStorageService : IStorageService
	{
		internal class Configuration
		{
			public string? StorageConnectionString { get; set; }
			public string? DestinationContainer { get; set; }

			public static Configuration Load(IConfiguration config)
			{
				var retVal = new Configuration();
				retVal.StorageConnectionString = config["StorageConnectionString"];
				retVal.DestinationContainer = config["DestinationContainer"];
				return retVal;
			}
		}

		private readonly IConfiguration configuration;
		private readonly ILogger<BlobStorageService> logger;

		public BlobStorageService(IConfiguration configuration,
			ILoggerFactory loggerFactory)
		{
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));
			if (loggerFactory == null)
				throw new ArgumentNullException(nameof(loggerFactory));

			this.configuration = configuration;
			this.logger = loggerFactory.CreateLogger<BlobStorageService>();
		}

		public async Task SerializeObjectToBlobAsync(object obj, string destinationName, CancellationToken cancellationToken)
		{
			var config = Configuration.Load(this.configuration);
			var blobServiceClient = new BlobServiceClient(config.StorageConnectionString);
			var containerClient = blobServiceClient.GetBlobContainerClient(config.DestinationContainer);
			await containerClient.UploadBlobAsync(destinationName,
				BinaryData.FromObjectAsJson(obj, 
					new JsonSerializerOptions() { WriteIndented = true }));
		}

		public async Task UploadToStorageAsync(Stream sourceStream, string destinationName, CancellationToken cancellationToken)
		{
			var config = Configuration.Load(this.configuration);
			var blobServiceClient = new BlobServiceClient(config.StorageConnectionString);
			var containerClient = blobServiceClient.GetBlobContainerClient(config.DestinationContainer);
			await containerClient.UploadBlobAsync(destinationName, sourceStream);
		}
	}
}
