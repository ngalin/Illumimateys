using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShadowWall
{
	public class HttpPointCloudWriter : IPointCloudWriter
	{
		public HttpPointCloudWriter()
		{
			webClient = new HttpClient();
		}

		public async Task WritePointCloudAsync(IEnumerable<PointFrame> pointCloud, int width, int height)
		{
			var serializedFrame = string.Join(";", pointCloud.Select(frame => frame.ToString()));
			var requestUri = string.Format("http://localhost:8084?width={0}&height={1}", width, height);
			var content = new StringContent(serializedFrame);
			await webClient.PostAsync(requestUri, content);
		}

		public void Dispose()
		{
			webClient.Dispose();
		}

		readonly HttpClient webClient;
	}
}
