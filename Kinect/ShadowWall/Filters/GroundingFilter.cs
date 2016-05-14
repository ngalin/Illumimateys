using System;
using System.Collections.Generic;

namespace ShadowWall.Filters
{
	public class GroundingFilter : IPointCloudFilter
	{
		public void Apply(IEnumerable<PointFrame> currentCloud)
		{
			var kinnectToFloorDistance = Math.Sqrt((Math.Pow(kinnectHeight, 2) + Math.Pow(minimumDistanceToWall, 2)));
			var floorSlopeAngle = Math.Asin(kinnectHeight / minimumDistanceToWall);

			foreach (var point in currentCloud)
			{
				TransformPoint(point, kinnectToFloorDistance, floorSlopeAngle);
			}
		}

		void TransformPoint(PointFrame point, double kinnectToFloorDistance, double floorSlopeAngle)
		{
			var pointAngle = Math.Asin(point.Y / point.Z);
			var newAngle = pointAngle - floorSlopeAngle;
			point.Z = (int)(point.Z * Math.Sin(newAngle));
			
		}

		#region Calibration

		readonly double kinnectHeight = 2;
		readonly double minimumDistanceToWall = 1.5;

		#endregion
	}
}
