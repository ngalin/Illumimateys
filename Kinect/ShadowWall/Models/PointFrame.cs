using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace ShadowWall
{
	public class PointFrame
	{
		public PointFrame() { }

		public PointFrame(PointFrame point)
		{
			X = point.X;
			Y = point.Y;
			Z = point.Z;
			R = point.R;
			G = point.G;
			B = point.B;
		}

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

		public static PointFrame[,] NewCloud(int width, int height)
		{
			var result = new PointFrame[width, height];
			for (int i = 0; i < width; ++i)
			{
				for (int j = 0; j < height; ++j)
				{
					result[i, j].X = i;
					result[i, j].Y = j;
					result[i, j].Z = -1;
					result[i, j].R = 0;
					result[i, j].G = 0;
					result[i, j].B = 0;
				}
			}
			return result;
		}
	}
}
