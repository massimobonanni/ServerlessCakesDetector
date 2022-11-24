using ServerlessCakesDetector.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessCakesDetector.Core.Interfaces
{
	public interface IImageAnalyzer
	{
		Task<ImageAnalyzerResult> AnalyzeImageAsync(Stream imageStream, CancellationToken cancellationToken = default);
		Task<ImageAnalyzerResult> AnalyzeImageAsync(string imageUrl, CancellationToken cancellationToken = default);

	}
}
