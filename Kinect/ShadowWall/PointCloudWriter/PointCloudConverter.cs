using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ShadowWall
{
	static class PointCloudConverter
	{
		public static Tuple<BitmapSource, byte[]> Convert(this IEnumerable<PointFrame> cloud, int width, int height)
		{
			var bytes = new byte[cloud.Count() * (PixelFormats.Bgr32.BitsPerPixel / 8)];
			byte[] result;
			var index = 0;
			foreach (var point in cloud)
			{
				bytes[index++] = (byte)point.B; // Blue
				bytes[index++] = (byte)point.G; // Green
				bytes[index++] = (byte)point.R; // Red
			}

			var source = BitmapSource.Create(width, height, 96, 96, PixelFormats.Rgb24, BitmapPalettes.Gray16, bytes, width * PixelFormats.Rgb24.BitsPerPixel / 8);
			var encoder = new GifBitmapEncoder();

			using (var stream = new MemoryStream())
			{
				encoder.Frames.Add(BitmapFrame.Create(source));
				encoder.Save(stream);

				result = stream.ToArray().Select(s => (byte)(sbyte)s).ToArray();
				stream.Close();
			}

			return Tuple.Create(source, result);
		}


		public static BitmapSource ConvertToBitmapSource(this byte[] bytes)
		{
			using (var stream = new MemoryStream(bytes))
			{
				return new GifBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.None).Frames[0];
			}
		}
	}
}
