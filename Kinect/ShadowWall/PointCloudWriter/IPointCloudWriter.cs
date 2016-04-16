using System.Collections.Generic;

namespace ShadowWall
{
	public interface IPointCloudWriter
	{
		void WritePointCloud(IEnumerable<PointFrame> pointCloud, int width, int height);
	}
}
