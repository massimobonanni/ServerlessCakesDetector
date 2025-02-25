using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Configuration
{
	internal static class ConfigurationExtensions
	{
		internal static bool IsEventGridConfigured(this IConfiguration config)
		{
			var topicEndpoint = config["TopicEndpoint"];
			var topicKey = config["TopicKey"];

			return !string.IsNullOrWhiteSpace(topicEndpoint) && !string.IsNullOrWhiteSpace(topicKey);
		}
	}
}
