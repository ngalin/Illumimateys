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
			var tuple = pointCloud.Convert(width, height);
			imageControl.Source = tuple.Item1;
			return Task.CompletedTask;
		}

		public void Dispose()
		{
		}

		Image imageControl;
	}
}
