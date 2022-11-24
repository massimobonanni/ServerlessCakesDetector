using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessCakesDetector.Cognitive.Configuration
{
	internal class ImageAnalyzerConfiguration
	{
		const string ConfigRootName = "ImageAnalyzer";
		public string? PredictionEndpoint { get; set; }
		public string? PredictionKey { get; set; }
		public string? ProjectId { get; set; }
		public string? ModelName { get; set; }
		public double Threshold { get; set; }

		public static ImageAnalyzerConfiguration Load(IConfiguration config)
		{
			var retVal = new ImageAnalyzerConfiguration();
			retVal.PredictionEndpoint = config[$"{ConfigRootName}:PredictionEndpoint"];
			retVal.PredictionKey = config[$"{ConfigRootName}:PredictionKey"];
			retVal.ProjectId = config[$"{ConfigRootName}:ProjectId"];
			retVal.ModelName = config[$"{ConfigRootName}:ModelName"];
			retVal.Threshold = config.GetValue<double>($"{ConfigRootName}:Threshold");
			return retVal;
		}
	}
}
