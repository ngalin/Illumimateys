using System;

namespace ShadowWall
{
	class AgingFilter : IPointCloudFilter
	{
		public PointFrame[,] Apply(PointFrame[,] currentCloud)
		{
			if (previousCloud == null)
			{
				previousCloud = PointFrame.NewCloud(currentCloud.GetLength(0), currentCloud.GetLength(1));
			}

			var resultCloud = PointFrame.NewCloud(currentCloud.GetLength(0), currentCloud.GetLength(1));
			if (previousCloud.Length != currentCloud.Length)
			{
				Array.Copy(currentCloud, resultCloud, currentCloud.Length);
			}
			else
			{
				for (var i = 0; i < currentCloud.GetLength(0); ++i)
				{
					for (var j = 0; j < currentCloud.GetLength(1); ++j)
					{
						var currentPoint = currentCloud[i, j];
						var previousPoint = previousCloud[i, j];
						var resultPoint = new PointFrame(currentPoint);

						if (!(currentPoint.Z < previousPoint.Z - minimumDelta || previousPoint.Z + minimumDelta < currentPoint.Z))
						{
							resultPoint.R = Math.Max(previousPoint.R - agingFactor, 1);
							resultPoint.G = Math.Max(previousPoint.G - agingFactor, 1);
							resultPoint.B = Math.Max(previousPoint.B - agingFactor, 1);
						}

						previousCloud[i, j] = resultPoint;
						resultCloud[i, j] = resultPoint;
					}
				}
			}
			
			return resultCloud;
		}

		#region Parameters
		int minimumDelta = 50;
		int agingFactor = 10;
		#endregion

		PointFrame[,] previousCloud;
	}
}
