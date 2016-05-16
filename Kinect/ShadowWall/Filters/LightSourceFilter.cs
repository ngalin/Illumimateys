using System.Collections.Generic;

namespace ShadowWall
{
	public class LightSourceFilter : IPointCloudFilter
	{
		public PointFrame[,] Apply(PointFrame[,] currentCloud)
		{
			var resultCloud = PointFrame.NewCloud(currentCloud.GetLength(0), currentCloud.GetLength(1));
			for (int i = 0; i < currentCloud.GetLength(0); ++i)
			{
				for (int j = 0; j < currentCloud.GetLength(1); ++j)
				{
					if (currentCloud[i,j].Z <= 0)
					{
						continue;
					}

					var newPoint = new PointFrame(currentCloud[i, j]);
					var projectionSlope = (lightSourceHeight - newPoint.Y) / (lightSourceDepth - newPoint.Z);
					var sourceIntersection = lightSourceHeight - (projectionSlope * lightSourceDepth);
					newPoint.Y = (1f * projectionSlope) + sourceIntersection;

					if (newPoint.Y > 0 && newPoint.Y < resultCloud.GetLength(1))
					{
						resultCloud[i, (int)newPoint.Y] = newPoint;
					}
				}
			}
			return resultCloud;
		}

		#region Calibration

		readonly int lightSourceDepth = 10000;
		readonly int lightSourceHeight = 700;

		#endregion
	}
}
