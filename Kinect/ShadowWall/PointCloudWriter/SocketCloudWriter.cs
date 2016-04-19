using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ShadowWall
{
	class SocketCloudWriter : IPointCloudWriter
	{
		const int port = 8084;
		const string ip = "127.0.0.1";

		public SocketCloudWriter()
		{
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Connect(ip, port);
		}

		Socket socket;

		public Task WritePointCloudAsync(IEnumerable<PointFrame> pointCloud, int width, int height)
		{
			socket.Send(pointCloud.ConvertToByteArray());
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			socket.Dispose();
		}
	}
}
