using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessCakesDetector.Core.Interfaces
{
	public interface IStorageService
	{
		Task UploadToStorageAsync(Stream sourceStream, string destinationName,CancellationToken cancellationToken);
		Task SerializeObjectToBlobAsync(object @obj, string destinationName,CancellationToken cancellationToken);

		Task<string> GetUrnAsync(string blobName, CancellationToken cancellationToken);
	}
}
