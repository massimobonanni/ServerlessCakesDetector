using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessCakesDetector.Functions.Requests
{
	public class AnalyzeImageFromStreamRequest
	{
		public int Id { get; set; }
		public string Description { get; set; }
		public byte[] Image { get; set; }
	}
}
