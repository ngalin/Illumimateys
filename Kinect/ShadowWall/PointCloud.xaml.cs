using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace ShadowWall
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class PointCloud : Window
	{
		public PointCloud()
		{
			InitializeComponent();

			this.Loaded += PointCloud_Loaded;
			this.Closed += PointCloud_Closed;
			this.KeyDown += PointCloud_KeyDown;
			this.MouseWheel += PointCloud_MouseWheel;

			var sensor = KinectSensor.GetDefault();
			sensor.Open();

			var depthReader = sensor.DepthFrameSource.OpenReader();
			depthReader.FrameArrived += depthReader_FrameArrived;

			var bodyReader = sensor.BodyFrameSource.OpenReader();
		}

		public int WallWidth { get { return 180; } }
		public int WallHeight { get { return 120; } }
		public int WallBreadth { get { return 800; } }

		private void PointCloud_Closed(object sender, EventArgs e)
		{
			KinectSensor.GetDefault().Close();
		}

		private void depthReader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
		{
			using (var frame = e.FrameReference.AcquireFrame())
			{
				if (frame != null)
				{
					var width = frame.FrameDescription.Width;
					var height = frame.FrameDescription.Height;
					var depths = new ushort[width * height];

					frame.CopyFrameDataToArray(depths);

					for (int i = 0; i < depths.Length; ++i)
					{
						depths[i] = depths[i] > (this.WallBreadth * 10) ? default(ushort) : depths[i];
					}

					this.Clear(Mesh);
					this.ConvertToPointCloud(depths, width, height);
					this.Flush();
				}
			}
		}

		private void ConvertToPointCloud(ushort[] array, int width, int height)
		{
			var points = new List<PointFrame>();

			for (int i = 0; i < array.Length; ++i)
			{
				var item = array[i];
				if (item > 0)
				{
					var x = (i % width) * this.WallWidth / (float)width;
					var y = (height - i / width) * this.WallHeight / (float)height;
					var z = item > 0 ? this.WallBreadth - (((float)item / (this.WallBreadth * 10)) * this.WallBreadth) : 0;

					var b = item / 3;
					var g = (item - b) / 3;
					var r = (item - b - g) / 3;

					points.Add(new PointFrame() {X = x, Y = y, Z = z, R = (byte)r, G = (byte)g, B = (byte)b });
				}
			}

			foreach (var point in points.GroupBy(p => new { X = (int)p.X, Y = (int)p.Y, Z = (int)p.Z }).Select(g => g.First()))
			{
				this.DrawPoint(Mesh, (int)point.X, (int)point.Y, (int)point.Z, (byte)point.R, (byte)point.G, (byte)point.B);
				//Serializer.Save((int)x, (int)y, (int)z, (byte)r, (byte)g, (byte)b);
			}
		}

		#region 3D

		private void PointCloud_Loaded(object sender, RoutedEventArgs e)
		{
		}

		private void PointCloud_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Up:
					RotateX.Angle += 10;
					break;
				case Key.Down:
					RotateX.Angle -= 10;
					break;
				case Key.Left:
					RotateY.Angle -= 10;
					break;
				case Key.Right:
					RotateY.Angle += 10;
					break;
				case Key.W:
					Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y - 50, Camera.Position.Z);
					break;
				case Key.S:
					Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y + 50, Camera.Position.Z);
					break;
				case Key.A:
					Camera.Position = new Point3D(Camera.Position.X + 50, Camera.Position.Y, Camera.Position.Z);
					break;
				case Key.D:
					Camera.Position = new Point3D(Camera.Position.X - 50, Camera.Position.Y, Camera.Position.Z);
					break;
				default:
					break;
			}
		}

		private void PointCloud_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta > 0)
			{
				Camera.Position = new Point3D(Camera.Position.X + 50, Camera.Position.Y - 50, Camera.Position.Z - 50);
			}
			else
			{
				Camera.Position = new Point3D(Camera.Position.X - 50, Camera.Position.Y + 50, Camera.Position.Z + 50);
			}
		}

		private void DrawPoint(MeshGeometry3D mesh, int x, int y, int z, byte r, byte g, byte b)
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

		private void Clear(MeshGeometry3D mesh)
		{
			mesh.Positions.Clear();
			mesh.TriangleIndices.Clear();
		}

		private void Flush()
		{
			Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
		}

		#endregion
	}
}
