using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ShadowWall
{
	class SocketCloudWriter : IPointCloudWriter
	{
		public SocketCloudWriter(Image image)
		{
			this.image = image;
		}

		readonly Image image;
		const int port = 8084;
		const string ip = "127.0.0.1";

		Socket TryConnectSocket()
		{
			try
			{
				var newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				newSocket.Connect(ip, port);
				return newSocket;
			}
			catch(Exception)
			{
				//Nomnom.
			}
			return null;
		}


		public Task WritePointCloudAsync(IEnumerable<PointFrame> pointCloud, int width, int height)
		{
			var socket = TryConnectSocket();
			if(socket == null)
			{
				return Task.CompletedTask;
			}
			try
			{
				var tuple = pointCloud.Convert(width, height);
				image.Source = tuple.Item1;
				var bytes = tuple.Item2;
				Console.WriteLine(bytes.Length);

				return Task.Factory.FromAsync(socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, OnAsyncCompleteCallback(socket), socket), OnAsyncCompleteAction(socket));
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

		private Action<IAsyncResult> OnAsyncCompleteAction(Socket socket)
		{
			return a =>
			{
				socket.Close();
				socket.Dispose();
			};
		}

		private AsyncCallback OnAsyncCompleteCallback(Socket socket)
		{
			return a =>
				{
					socket.Close();
					socket.Dispose();
				};
		}


		public void Dispose()
		{

		}
	}
}
