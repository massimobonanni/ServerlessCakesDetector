using ServerlessCakesDetector.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models
{
	internal static class BoundingBoxExtensions
	{
		internal static ImageRectangle ToImageRectangle(this BoundingBox box)
		{
			return new ImageRectangle() { 
			 Height= (int)Math.Ceiling(box.Height),
			 Left= (int)Math.Ceiling(box.Left),
			 Top= (int)Math.Ceiling(box.Top),
			 Width= (int)Math.Ceiling(box.Width)
			};
		}
	}
}
