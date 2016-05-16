using System.Collections.Generic;

namespace ShadowWall
{
	public interface IPointCloudFilter
	{
		PointFrame[,] Apply(PointFrame[,] currentCloud);
	}
}
