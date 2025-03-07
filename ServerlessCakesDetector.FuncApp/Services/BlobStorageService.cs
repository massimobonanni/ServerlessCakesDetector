﻿using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServerlessCakesDetector.Core.Interfaces;
using System.Text.Json;

namespace ServerlessCakesDetector.FuncApp.Services
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
            logger = loggerFactory.CreateLogger<BlobStorageService>();
		}

		public async Task SerializeObjectToBlobAsync(object obj, string destinationName, CancellationToken cancellationToken)
		{
			var containerClient = CreateBlobContainerClient();
			await containerClient.UploadBlobAsync(destinationName,
				BinaryData.FromObjectAsJson(obj,
					new JsonSerializerOptions() { WriteIndented = true }));
		}

		public async Task UploadToStorageAsync(Stream sourceStream, string destinationName, CancellationToken cancellationToken)
		{
			var containerClient = CreateBlobContainerClient();
			await containerClient.UploadBlobAsync(destinationName, sourceStream);
		}

		public Task<string> GetUrnAsync(string blobName, CancellationToken cancellationToken)
		{
			var containerClient = CreateBlobContainerClient();
			var blobClient = containerClient.GetBlobClient(blobName);
			var sasUrl = blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read,
				DateTimeOffset.Now.AddDays(1));

			return Task.FromResult(sasUrl.ToString());
		}
		private BlobContainerClient CreateBlobContainerClient()
		{
			var config = Configuration.Load(configuration);
			var blobServiceClient = new BlobServiceClient(config.StorageConnectionString);
			var containerClient = blobServiceClient.GetBlobContainerClient(config.DestinationContainer);
			return containerClient;
		}
	}
}
