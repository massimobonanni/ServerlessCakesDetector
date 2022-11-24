using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessCakesDetector.Core.Models
{
	public class ImageAnalyzerResult
	{
		public long ElapsedTimeInMilliseconds { get; set; }

		public int NumberOfObjects => Objects.Count;

		public List<ObjectInfo> Objects { get; set; } = new List<ObjectInfo>();
	}
}
