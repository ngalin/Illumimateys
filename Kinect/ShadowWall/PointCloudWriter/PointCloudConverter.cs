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
		public static byte[] ConvertToByteArray(this IEnumerable<PointFrame> cloud, int width, int height)
		{
			var bytes = new byte[cloud.Count() * (PixelFormats.Bgr32.BitsPerPixel / 8)];
			byte[] result;
			var index = 0;
			foreach (var point in cloud)
			{
				bytes[index++] = (byte)point.B; // Blue
				bytes[index++] = (byte)point.G; // Green
				bytes[index++] = (byte)point.B; // Red
				bytes[index++] = 0; // Alpha
			}

			var source = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, BitmapPalettes.WebPalette, bytes, width * PixelFormats.Bgr32.BitsPerPixel / 8);
			var encoder = new GifBitmapEncoder();

			using (var stream = new MemoryStream())
			{
				encoder.Frames.Add(BitmapFrame.Create(source));
				encoder.Save(stream);

				result = stream.ToArray();
				stream.Close();
			}

			return result;
		}

		public static byte[] ImageToByte2(Image img)
		{
			byte[] byteArray = new byte[0];
			using (MemoryStream stream = new MemoryStream())
			{
				img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
				stream.Close();

				byteArray = stream.ToArray();
			}
			return byteArray;
		}

		static System.Drawing.Bitmap BitmapFromSource(BitmapSource bitmapsource)
		{
			System.Drawing.Bitmap bitmap;
			using (MemoryStream outStream = new MemoryStream())
			{
				BitmapEncoder enc = new BmpBitmapEncoder();
				enc.Frames.Add(BitmapFrame.Create(bitmapsource));
				enc.Save(outStream);
				bitmap = new System.Drawing.Bitmap(outStream);
			}
			return bitmap;
		}
	}
}
