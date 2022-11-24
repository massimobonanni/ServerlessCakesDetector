using ServerlessCakesDetector.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessCakesDetector.Core.Interfaces
{
	public interface IImageProcessor
	{
		Task CropImageAsync(Stream sourceImage, ImageRectangle rectangle, Stream outputImage);

	}
}
