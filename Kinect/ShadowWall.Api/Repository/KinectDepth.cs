using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Kinect;

namespace ShadowWall.Api.Repository
{
	public class KinectDepth
	{
		public KinectDepth()
		{
			//var sensor = KinectSensor.GetDefault();
			//sensor.Open();

			//var depthReader = sensor.DepthFrameSource.OpenReader();
			//depthReader.FrameArrived += depthReader_FrameArrived;
		}

		public IEnumerable<PointFrame> GetPoints()
		{
			return Serializer.LoadPoint(@"C:\Work\Git\Illumimateys\Kinect\ShadowWall.Api\Resources\ShadowWall.exe.csv");
		}

		//public int WallWidth { get { return 180; } }
		//public int WallHeight { get { return 120; } }
		//public int WallBreadth { get { return 800; } }

		//public Func<IEnumerable<PointFrame>, IEnumerable<PointFrame>> Rendered;

		//private void depthReader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
		//{
		//	using (var frame = e.FrameReference.AcquireFrame())
		//	{
		//		if (frame != null)
		//		{
		//			var width = frame.FrameDescription.Width;
		//			var height = frame.FrameDescription.Height;
		//			var depths = new ushort[width * height];

		//			frame.CopyFrameDataToArray(depths);

		//			for (int i = 0; i < depths.Length; ++i)
		//			{
		//				depths[i] = depths[i] > (this.WallBreadth * 10) ? default(ushort) : depths[i];
		//			}

		//			if (this.Rendered != null)
		//			{
		//				this.Rendered(this.ConvertToPointCloud(depths, width, height).ToList());
		//			}
		//		}
		//	}
		//}

		//private IEnumerable<PointFrame> ConvertToPointCloud(ushort[] array, int width, int height)
		//{
			//var points = new List<PointFrame>();

			//for (int i = 0; i < array.Length; ++i)
			//{
			//	var item = array[i];
			//	if (item > 0)
			//	{
			//		var x = (i % width) * this.WallWidth / (float)width;
			//		var y = (height - i / width) * this.WallHeight / (float)height;
			//		var z = item > 0 ? this.WallBreadth - (((float)item / (this.WallBreadth * 10)) * this.WallBreadth) : 0;

			//		var b = item / 3;
			//		var g = (item - b) / 3;
			//		var r = (item - b - g) / 3;

			//		points.Add(new PointFrame() { X = x, Y = y, Z = z, R = (byte)r, G = (byte)g, B = (byte)b });
			//	}
			//}

			//var distinctPoints = points.GroupBy(p => new { X = (int)p.X, Y = (int)p.Y, Z = (int)p.Z }).Select(g => g.First());
			//foreach (var point in distinctPoints)
			//{
			//	yield return point;
			//}
		//}
	}
}