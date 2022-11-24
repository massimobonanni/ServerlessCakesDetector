using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServerlessCakesDetector.Cognitive.Services;
using ServerlessCakesDetector.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

[assembly: FunctionsStartup(typeof(ServerlessCakesDetector.Functions.Startup))]

namespace ServerlessCakesDetector.Functions
{
	public class Startup : FunctionsStartup
	{
		public override void Configure(IFunctionsHostBuilder builder)
		{
			builder.Services.AddScoped<IImageAnalyzer, CustomVisionImageAnalyzer>();
		}
	}
}
