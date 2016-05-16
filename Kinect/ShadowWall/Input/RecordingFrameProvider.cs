using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowWall.Input
{
	public class RecordingFrameProvider : IFrameProvider
	{
		public RecordingFrameProvider(string fileName)
		{
			fileStream = File.OpenRead(fileName);
			reader = new StreamReader(fileStream);

			new Task(ProvideFrames).Start();
		}

		void ProvideFrames()
		{
			while (!reader.EndOfStream)
			{
				var frame = reader.ReadLine();
				var depths = frame.Split(',').Select(depthStr => ushort.Parse(depthStr)).ToArray();
				FrameArrived(null, new FrameArrivedArgs(depths, 640, 480));
				Thread.Sleep(35);
			}
		}

		public event EventHandler<FrameArrivedArgs> FrameArrived;

		public void Dispose()
		{
			reader.Dispose();
			fileStream.Dispose();
		}

		Stream fileStream;
		StreamReader reader;
	}
}
