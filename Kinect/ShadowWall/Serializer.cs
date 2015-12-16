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
		public static void Save(Body body)
		{
			using (var file = new FileStream(FilePath, FileMode.Append))
			{
				using (var stream = new StreamWriter(file))
				{
					stream.WriteLine(string.Join(",", body.Joints.Select(j => string.Format("{0}|{1}|{2}|{3}", j.Value.JointType, j.Value.Position.X, j.Value.Position.Y, j.Value.Position.Z))));
				}
			}
		}

		public static IEnumerable<BodyFrame> Load()
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

		private static string FilePath
		{
			get
			{
				var location = Assembly.GetExecutingAssembly().Location;
				return location.Substring(0, location.LastIndexOf(@"\")) + @"\Resources\ShadowWall.exe.csv";
			}
		}
	}
}
