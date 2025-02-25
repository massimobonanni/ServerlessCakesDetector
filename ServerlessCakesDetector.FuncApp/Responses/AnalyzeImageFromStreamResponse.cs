using ServerlessCakesDetector.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessCakesDetector.FuncApp.Responses
{
	public class AnalyzeImageFromStreamResponse
	{
		public string OperationId { get; set; }
		public string OriginalFileName { get; set; }
		public string FileName { get; set; }
		public ImageAnalyzerResult AnalysisResult { get; set; }

		public List<ObjectBlob> ObjectsBlobs { get; set; }
	}

	public class ObjectBlob
	{
		public string ObjectId { get; set; }
		public string BlobUrl { get; set; }
	}
}
