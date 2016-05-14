using System.Collections.Generic;

namespace ShadowWall.Filters
{
	public class LightSourceFilter : IPointCloudFilter
	{
		public void Apply(IEnumerable<PointFrame> currentCloud)
		{
			foreach (var point in currentCloud)
			{
				var projectionSlope = (lightSourceHeight - point.Y) / (lightSourceDepth - point.Z);
				point.Y = lightSourceHeight - (projectionSlope * lightSourceDepth);
			}
		}

		#region Calibration

		readonly int lightSourceDepth = 10;
		readonly int lightSourceHeight = 2;

		#endregion
	}
}
