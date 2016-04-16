using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShadowWall
{
	public interface IPointCloudWriter : IDisposable
	{
		Task WritePointCloudAsync(IEnumerable<PointFrame> pointCloud, int width, int height);
	}
}
