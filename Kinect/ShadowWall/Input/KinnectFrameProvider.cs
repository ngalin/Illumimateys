using Microsoft.Kinect;
using System;

namespace ShadowWall
{
	public class KinnectFrameProvider : IFrameProvider
	{
		public KinnectFrameProvider()
		{
			var sensor = KinectSensor.GetDefault();
			sensor.Open();

			depthReader = sensor.DepthFrameSource.OpenReader();
			depthReader.FrameArrived += KinnectFrameArrived;
		}

		void KinnectFrameArrived(object sender, DepthFrameArrivedEventArgs e)
		{
			using (var frame = e.FrameReference.AcquireFrame())
			{
				if (frame != null)
				{
					var width = frame.FrameDescription.Width;
					var height = frame.FrameDescription.Height;
					var depths = new ushort[width * height];

					frame.CopyFrameDataToArray(depths);

					FrameArrived(sender, new FrameArrivedArgs(depths, width, height));
				}
			}
		}

		public event EventHandler<FrameArrivedArgs> FrameArrived;

		public void Dispose()
		{
			depthReader.Dispose();
			sensor.Close();
		}

		DepthFrameReader depthReader;
		KinectSensor sensor;
	}
}
