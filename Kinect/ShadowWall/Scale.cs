using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowWall
{
	public static class Scale
	{
		public static double X(double x, double width)
		{
			return (width / 2) + (x * (width / 4));
		}

		public static double Y(double y, double height)
		{
			return (height / 2.5) + (-y * (height / 2.5));
		}
	}
}
