using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowWall
{
	public class RecordingFrameProvider : IFrameProvider
	{
		public RecordingFrameProvider(string fileName)
		{
			fileStream = File.OpenRead(fileName);

			new Task(ProvideFrames).Start();
		}

		void ProvideFrames()
		{
			var currentFrame = new List<ushort>();
			var currentNumber = new StringBuilder();
			int currentByte;
			while ((currentByte = fileStream.ReadByte()) != -1)
			{
				var character = (char)currentByte;
				if (character == ',')
				{
					currentFrame.Add(ushort.Parse(currentNumber.ToString()));
					currentNumber = new StringBuilder();

					if (currentFrame.Count == (recordedWidth * recordedHeight) - 1)
					{
						FrameArrived(null, new FrameArrivedArgs(currentFrame.ToArray(), recordedWidth, recordedHeight));
						currentFrame = new List<ushort>();
						Thread.Sleep(33);
					}
				}
				else
				{
					currentNumber.Append(character);
				}
			}
		}

		public event EventHandler<FrameArrivedArgs> FrameArrived;

		public void Dispose()
		{
			fileStream.Dispose();
		}

		const int recordedWidth = 512;
		const int recordedHeight = 424;
		Stream fileStream;
	}
}
