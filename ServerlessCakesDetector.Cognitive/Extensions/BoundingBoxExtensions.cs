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
			 Height= box.Height,
			 Left= box.Left,
			 Top= box.Top,
			 Width= box.Width
			};
		}
	}
}
