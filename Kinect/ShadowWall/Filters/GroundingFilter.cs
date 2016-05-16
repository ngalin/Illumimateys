using System;

namespace ShadowWall.Filters
{
	public class GroundingFilter : IPointCloudFilter
	{
		public PointFrame[,] Apply(PointFrame[,] currentCloud)
		{
			var resultCloud = PointFrame.NewCloud(currentCloud.GetLength(0), currentCloud.GetLength(1));
			var kinnectToFloorDistance = Math.Sqrt((Math.Pow(kinnectHeight, 2) + Math.Pow(minimumDistanceToWall, 2)));
			var floorSlopeAngle = Math.Asin(kinnectHeight / minimumDistanceToWall);

			for (int i = 0; i < currentCloud.GetLength(0); ++i)
			{
				for (int j = 0; j < currentCloud.GetLength(1); ++j)
				{
					if (currentCloud[i,j].Z == -1)
					{
						continue;
					}

					var newPoint = TransformPoint(currentCloud[i, j], kinnectToFloorDistance, floorSlopeAngle);
					if (newPoint.Y >= 0)
					{
						resultCloud[(int)newPoint.X, (int)newPoint.Y] = newPoint;
					}
				}
			}

			return resultCloud;
		}

		PointFrame TransformPoint(PointFrame point, double kinnectToFloorDistance, double floorSlopeAngle)
		{
			var resultPoint = new PointFrame(point);
			var pointAngle = Math.Asin(resultPoint.Y / resultPoint.Z);
			var newAngle = pointAngle - floorSlopeAngle;
			resultPoint.Y = (int)(resultPoint.Z * Math.Sin(newAngle));
			return resultPoint;
		}

		#region Calibration

		readonly double kinnectHeight = 2;
		readonly double minimumDistanceToWall = 1.5;

		#endregion
	}
}
