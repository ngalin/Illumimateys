using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace ShadowWall
{
	public class Serializer
	{
		public Serializer()
		{
		}

		public Serializer(string fileSuffix)
		{
			FileSuffix = fileSuffix;
		}

		public void Save(Body body)
		{
			using (var file = new FileStream(FilePath, FileMode.Append))
			{
				using (var stream = new StreamWriter(file))
				{
					stream.WriteLine(string.Join(",", body.Joints.Select(j => string.Format("{0}|{1}|{2}|{3}", j.Value.JointType, j.Value.Position.X, j.Value.Position.Y, j.Value.Position.Z))));
				}
			}
		}

		public void Save(int x, int y, int z, byte r, byte g, byte b)
		{
			using (var file = new FileStream(FilePath, FileMode.Append))
			{
				using (var stream = new StreamWriter(file))
				{
					stream.WriteLine(string.Format("{0}|{1}|{2}|{3}|{4}|{5}", x, y, z, r, g, b));
				}
			}
		}

		public IEnumerable<BodyFrame> LoadSkeleton()
		{
			using (var file = new FileStream(FilePath, FileMode.Open))
			{
				using (var stream = new StreamReader(file))
				{
					string bodySerialized;
					while ((bodySerialized = stream.ReadLine()) != null)
					{
						var body = new BodyFrame();
						var jointsSerialized = bodySerialized.Split(new char[] { ',' });
						foreach (var jointSerialized in jointsSerialized)
						{
							var jointProperties = jointSerialized.Split(new char[] { '|' });
							body.Joints.Add(new Joint { JointType = (JointType)Enum.Parse(typeof(JointType), jointProperties[0]), Position = new CameraSpacePoint { X = float.Parse(jointProperties[1]), Y = float.Parse(jointProperties[2]), Z = float.Parse(jointProperties[3]) } });
						}

						yield return body;
					}
				}
			}
		}

		public IEnumerable<PointFrame> LoadPoint()
		{
			using (var file = new FileStream(FilePath, FileMode.Open))
			{
				using (var stream = new StreamReader(file))
				{
					string pointSerialized;
					while ((pointSerialized = stream.ReadLine()) != null)
					{
						var pointProperties = pointSerialized.Split(new char[] { '|' });
						yield return new PointFrame { X = float.Parse(pointProperties[0]), Y = float.Parse(pointProperties[1]), Z = float.Parse(pointProperties[2]), R = int.Parse(pointProperties[3]), G = int.Parse(pointProperties[4]), B = int.Parse(pointProperties[5]) };
					}
				}
			}
		}

		private string FilePath
		{
			get
			{
				var location = Assembly.GetExecutingAssembly().Location;
				return location.Substring(0, location.LastIndexOf(@"\")) + @"\ShadowWallSnapshot" + FileSuffix + ".csv";
			}
		}

		private string FileSuffix { get; set; }
	}
}
