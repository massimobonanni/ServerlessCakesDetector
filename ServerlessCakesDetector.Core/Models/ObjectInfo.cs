using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessCakesDetector.Core.Models
{
	public class ObjectInfo
	{
		public string Id { get; set; }
		public List<string> Tags { get; set; }
		public ImageRectangle Rectangle { get; set; }
	}
}
