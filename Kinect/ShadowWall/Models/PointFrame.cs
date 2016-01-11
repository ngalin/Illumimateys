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
	}
}
