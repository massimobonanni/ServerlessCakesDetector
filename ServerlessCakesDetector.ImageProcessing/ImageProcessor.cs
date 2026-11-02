using ServerlessCakesDetector.Core.Interfaces;
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
			using Image img = await Image.LoadAsync(sourceImage, cancellationToken);
			IImageFormat format = img.Metadata.DecodedImageFormat!;
			var cropRectangle = new Rectangle()
			{
				X = (int)Math.Ceiling(img.Width * rectangle.Left),
				Y = (int)Math.Ceiling(img.Height * rectangle.Top),
				Height = (int)Math.Ceiling(img.Height * rectangle.Height),
				Width = (int)Math.Ceiling(img.Width * rectangle.Width)
			};

			img.Mutate(x => x.Crop(cropRectangle));

			await img.SaveAsync(outputImage, format, cancellationToken);
		}

		public async Task<Stream> CropImageAsync(Stream sourceImage, ImageRectangle rectangle,
			CancellationToken cancellationToken)
		{
			MemoryStream outStream = new MemoryStream();

			using Image img = await Image.LoadAsync(sourceImage, cancellationToken);
			IImageFormat format = img.Metadata.DecodedImageFormat!;

			var cropRectangle = new Rectangle()
			{
				X = (int)Math.Ceiling(img.Width * rectangle.Left),
				Y = (int)Math.Ceiling(img.Height * rectangle.Top),
				Height = (int)Math.Ceiling(img.Height * rectangle.Height),
				Width = (int)Math.Ceiling(img.Width * rectangle.Width)
			};

			img.Mutate(x => x.Crop(cropRectangle));

			await img.SaveAsync(outStream, format, cancellationToken);

			outStream.Position = 0;
			return outStream;
		}
	}
}
