﻿using ServerlessCakesDetector.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessCakesDetector.Core.Interfaces
{
	public interface ICakesDetector
	{
		Task<CakesDetectionResult> DetectImageAsync(Stream imageData, CancellationToken token = default);

		Task<CakesDetectionResult> DetectImageUrlAsync(string imageUrl, CancellationToken token = default);
	}
}
