using System;
using System.Collections.Generic;
using System.Linq;

namespace ShadowWall.Filters
{
	public class GroundingFilter : IPointCloudFilter
	{
		public void Apply(IEnumerable<PointFrame> currentCloud)
		{
			var kinnectToFloorDistance = Math.Sqrt((Math.Pow(kinnectHeight, 2) + Math.Pow(minimumDistanceToWall, 2)));
			var floorSlopeAngle = Math.Asin(kinnectHeight / minimumDistanceToWall);

			currentCloud = currentCloud.Select(point => TransformPoint(point, kinnectToFloorDistance, floorSlopeAngle));
		}

		PointFrame TransformPoint(PointFrame point, double kinnectToFloorDistance, double floorSlopeAngle)
		{
			var pointAngle = Math.Asin(point.Y / point.Z);
			var newAngle = pointAngle - floorSlopeAngle;
			var newPointY = point.Z * Math.Sin(newAngle);

			return new PointFrame()
			{
				R = point.R,
				G = point.G,
				B = point.B,
				X = point.X,
				Y = (int)newPointY,
				Z = point.Z
			};
			
		}

		#region Calibration

		readonly double kinnectHeight = 2;
		readonly double minimumDistanceToWall = 1.5;

		#endregion
	}
}
