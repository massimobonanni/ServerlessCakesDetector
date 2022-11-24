using Microsoft.WindowsAzure.Storage.Blob;
using ServerlessCakesDetector.Core.Interfaces;
using ServerlessCakesDetector.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Microsoft.AspNetCore.Http
{
	public static class HttpExtensions
	{
		public static async Task UploadToStorageAsync(this IFormFile file,
			IStorageService storageService, string blobName,
			CancellationToken token = default)
		{
			using (var sourceStream = file.OpenReadStream())
			{
				await storageService.UploadToStorageAsync(sourceStream, blobName, token);
			}
		}
		public static async Task<ImageAnalyzerResult> AnalyzeAsync(this IFormFile file,
			IImageAnalyzer imageAnalyzer,
			CancellationToken token = default)
		{
			ImageAnalyzerResult analysisResult;
			using (var sourceStream = file.OpenReadStream())
			{
				analysisResult = await imageAnalyzer.AnalyzeImageAsync(sourceStream, token);
			}
			return analysisResult;
		}

		public static bool IsValid(this HttpRequest req)
		{
			if (!req.ContentType.StartsWith("multipart/form-data"))
				return false;
			if (req.Form == null || req.Form.Files == null || req.Form.Files.Count == 0)
				return false;
			return true;
		}
	}
}
