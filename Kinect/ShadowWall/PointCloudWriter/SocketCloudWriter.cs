using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowWall
{
	class SocketCloudWriter : IPointCloudWriter
	{
		public SocketCloudWriter(Action<byte[]> callback = null)
		{
			this.callback = callback;
		}

		readonly Action<byte[]> callback;
		const int port = 8084;
		const string ip = "127.0.0.1";

		bool TryConnectSocket()
		{
			try
			{
				var newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				newSocket.Connect(ip, port);
				socket = newSocket;
				return true;
			}
			catch(Exception)
			{
				//Nomnom.
			}
			return false;
		}

		Socket socket;

		public Task WritePointCloudAsync(IEnumerable<PointFrame> pointCloud, int width, int height)
		{
			if(socket == null)
			{
				TryConnectSocket();
			}
			try
			{
				var bytes = pointCloud.ConvertToByteArray(width, height);
				if (callback != null)
				{
					callback(bytes);
				}
				return Task.Factory.FromAsync(socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, OnAsyncComplete, socket), OnAsyncComplete);
			}
			catch (Exception)
			{
				if (socket != null)
				{
					socket.Dispose();
					socket = null;
				}
			}
				
			return Task.CompletedTask;
		}

		private void OnAsyncComplete(IAsyncResult ar)
		{
		}

		public void Dispose()
		{
			socket.Dispose();
		}
	}
}
