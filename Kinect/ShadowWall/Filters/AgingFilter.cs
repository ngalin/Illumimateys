using System;
using System.Collections.Generic;
using System.Linq;

namespace ShadowWall
{
	class AgingFilter : IPointCloudFilter
	{
		public void Apply(IEnumerable<PointFrame> currentCloud)
		{
			if (previousCloud == null || previousCloud.Count() != currentCloud.Count())
			{
				previousCloud = new List<PointFrame>(currentCloud).ToArray();
				return;
			}

			var index = 0;
			foreach(var currentPoint in currentCloud)
			{
				var previousPoint = previousCloud[index];
				if (currentPoint.Z < previousPoint.Z - minimumDelta || previousPoint.Z + minimumDelta < currentPoint.Z)
				{
					previousPoint.Z = currentPoint.Z;
				}
				else
				{
					currentPoint.R = Math.Max(previousPoint.R - agingFactor, 0);
					currentPoint.G = Math.Max(previousPoint.G - agingFactor, 0);
					currentPoint.B = Math.Max(previousPoint.B - agingFactor, 0);
				}

				previousPoint.R = currentPoint.R;
				previousPoint.G = currentPoint.G;
				previousPoint.B = currentPoint.B;

				++index;
			}
		}

		#region Parameters
		int minimumDelta = 100;
		int agingFactor = 10;
		#endregion

		PointFrame[] previousCloud;
	}
}
