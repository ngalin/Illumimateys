using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace ShadowWall
{
	public class PointFrame
	{
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }
		public int R { get; set; }
		public int G { get; set; }
		public int B { get; set; }

		public override string ToString()
		{
			return string.Format("{0},{1},{2},{3},{4},{5},{6}", X, Y, Z, R, G, B);
		}
	}
}
