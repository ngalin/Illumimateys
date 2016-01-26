using System.Collections.Generic;

namespace ShadowWall
{
	public interface IPointCloudFilter
	{
		void Apply(IEnumerable<PointFrame> currentCloud);
	}
}
