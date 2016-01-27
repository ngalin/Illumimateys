using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace ShadowWall.Api
{
	public class Serializer
	{
		public static void Save(int x, int y, int z, byte r, byte g, byte b)
		{
			using (var file = new FileStream(FilePath, FileMode.Append))
			{
				using (var stream = new StreamWriter(file))
				{
					stream.WriteLine(string.Format("{0}|{1}|{2}|{3}|{4}|{5}", x, y, z, r, g, b));
				}
			}
		}

		public static IEnumerable<PointFrame> LoadPoint(string filePath = null)
		{
			using (var file = new FileStream(filePath ?? FilePath, FileMode.Open))
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
