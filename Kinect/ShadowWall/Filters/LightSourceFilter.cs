using System.Collections.Generic;

namespace ShadowWall.Filters
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
					if (currentCloud[i,j].Z == -1)
					{
						continue;
					}

					var newPoint = new PointFrame(currentCloud[i, j]);
					var projectionSlope = (lightSourceHeight - newPoint.Y) / (lightSourceDepth - newPoint.Z);
					newPoint.Y = lightSourceHeight - (projectionSlope * lightSourceDepth);

					if (newPoint.Y > 0)
					{
						resultCloud[i, (int)newPoint.Y] = newPoint;
					}
				}
			}
			return resultCloud;
		}

		#region Calibration

		readonly int lightSourceDepth = 10;
		readonly int lightSourceHeight = 2;

		#endregion
	}
}
