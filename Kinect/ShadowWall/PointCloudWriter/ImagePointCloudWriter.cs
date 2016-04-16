using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ShadowWall
{
	class ImagePointCloudWriter : IPointCloudWriter
	{
		public ImagePointCloudWriter(Image targetImage)
		{
			imageControl = targetImage;
		}

		public Task WritePointCloudAsync(IEnumerable<PointFrame> pointCloud, int width, int height)
		{
			var byteCloud = ConvertToByteArray(pointCloud);
			imageControl.Source = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, BitmapPalettes.WebPalette, byteCloud, width * PixelFormats.Bgr32.BitsPerPixel / 8); ;
			return Task.CompletedTask;
		}

		public void Dispose()
		{
		}

		byte[] ConvertToByteArray(IEnumerable<PointFrame> cloud)
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

		Image imageControl;
	}
}
