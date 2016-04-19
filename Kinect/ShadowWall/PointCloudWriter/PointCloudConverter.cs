using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ShadowWall
{
	static class PointCloudConverter
	{
		public static byte[] ConvertToByteArray(this IEnumerable<PointFrame> cloud)
		{
			var bytes = new byte[cloud.Count() * (PixelFormats.Bgr32.BitsPerPixel / 8)];

			var index = 0;
			foreach (var point in cloud)
			{
				bytes[index++] = (byte)point.B; // Blue
				bytes[index++] = (byte)point.G; // Green
				bytes[index++] = (byte)point.B; // Red
				bytes[index++] = 0; // Alpha
			}

			return bytes;
		}
	}
}
