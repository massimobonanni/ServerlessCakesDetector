﻿using ServerlessCakesDetector.Core.Interfaces;
using ServerlessCakesDetector.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessCakesDetector.ImageProcessing
{
	public class ImageProcessor : IImageProcessor
	{
		public async Task CropImageAsync(Stream sourceImage, ImageRectangle rectangle, 
			Stream outputImage, CancellationToken cancellationToken)
		{
			(Image Image, IImageFormat Format) imf = 
				await Image.LoadWithFormatAsync(sourceImage,cancellationToken);

			using Image img = imf.Image;

			img.Mutate(x =>
				  x.Crop(
					  new Rectangle(rectangle.Left, rectangle.Top, 
									rectangle.Width, rectangle.Height)));

			await img.SaveAsync(outputImage, imf.Format,cancellationToken);
		}
	}
}