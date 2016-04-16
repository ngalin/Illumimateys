using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ShadowWall
{
	public class MeshGeometryPointCloudWriter : IPointCloudWriter
	{
		public MeshGeometryPointCloudWriter(MeshGeometry3D mesh)
		{
			targetMesh = mesh;
		}

		public Task WritePointCloudAsync(IEnumerable<PointFrame> pointCloud, int width, int height)
		{
			foreach (var point in pointCloud)
			{
				if (point.Z > 0)
				{
					DrawPoint(targetMesh, (int)point.X, (int)point.Y, (int)point.Z, (byte)point.R, (byte)point.G, (byte)point.B);
				}
			}

			return Task.CompletedTask;
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		void DrawPoint(MeshGeometry3D mesh, int x, int y, int z, byte r, byte g, byte b)
		{
			var count = mesh.Positions.Count;
			var color = new Color() { A = 0, R = r, G = g, B = b };

			mesh.Positions.Add(new Point3D(x - 0.5, y - 0.5, z + 0.5));
			mesh.Positions.Add(new Point3D(x + 0.5, y + 0.5, z + 0.5));
			mesh.Positions.Add(new Point3D(x - 0.5, y + 0.5, z + 0.5));

			mesh.TriangleIndices.Add(0 + count);
			mesh.TriangleIndices.Add(1 + count);
			mesh.TriangleIndices.Add(2 + count);
		}

		readonly MeshGeometry3D targetMesh;
	}
}
