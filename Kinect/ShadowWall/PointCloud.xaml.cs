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
		}

		public int Distance { get { return 2000; } }

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
						depths[i] = depths[i] > Distance ? default(ushort) : depths[i];
					}

					Mesh.Positions.Clear();
					Mesh.TriangleIndices.Clear();

					this.ConvertToPointCloud(depths, width, height);

					Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
				}
			}
		}

		private void PointCloud_Closed(object sender, EventArgs e)
		{
			KinectSensor.GetDefault().Close();
		}

		private void ConvertToPointCloud(ushort[] array, int width, int height)
		{
			for (int i = 0; i < array.Length; ++i)
			{
				var item = array[i];
				if (item > 0)
				{
					var x = i % width;
					var y = height - i / width;
					var z = item > 0 ? width - (((float)item / this.Distance) * width) : 0;

					var blue = item / 3;
					var green = (item - blue) / 3;
					var red = (item - blue - green) / 3;

					this.DrawCube(new Point3D(x, y, z), new Color() { A = 0, R = (byte)red, G = (byte)green, B = (byte)blue });
				}
			}
		}

		#region 3D

		private void PointCloud_Loaded(object sender, RoutedEventArgs e)
		{
			//Camera.Position = new Point3D(Camera.Position.X + 256, Camera.Position.Y + 212, Camera.Position.Z);
			//this.DrawCube(new Point3D(0, 0, 0));
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

		private void DrawCube(Point3D point, Color color)
		{
			var mesh = Mesh; // new MeshGeometry3D();

			var count = mesh.Positions.Count;

			mesh.Positions.Add(new Point3D(point.X - 0.5, point.Y - 0.5, point.Z + 0.5));
			//mesh.Positions.Add(new Point3D(point.X + 0.5, point.Y - 0.5, point.Z + 0.5));
			mesh.Positions.Add(new Point3D(point.X + 0.5, point.Y + 0.5, point.Z + 0.5));
			mesh.Positions.Add(new Point3D(point.X - 0.5, point.Y + 0.5, point.Z + 0.5));

			//mesh.Positions.Add(new Point3D(point.X - 0.5, point.Y - 0.5, point.Z - 0.5));
			//mesh.Positions.Add(new Point3D(point.X + 0.5, point.Y - 0.5, point.Z - 0.5));
			//mesh.Positions.Add(new Point3D(point.X + 0.5, point.Y + 0.5, point.Z - 0.5));
			//mesh.Positions.Add(new Point3D(point.X - 0.5, point.Y + 0.5, point.Z - 0.5));

			mesh.TriangleIndices.Add(0 + count);
			mesh.TriangleIndices.Add(1 + count);
			mesh.TriangleIndices.Add(2 + count);
			//mesh.TriangleIndices.Add(0 + count);
			//mesh.TriangleIndices.Add(2 + count);
			//mesh.TriangleIndices.Add(3 + count);

			//mesh.TriangleIndices.Add(4 + count);
			//mesh.TriangleIndices.Add(0 + count);
			//mesh.TriangleIndices.Add(7 + count);
			//mesh.TriangleIndices.Add(0 + count);
			//mesh.TriangleIndices.Add(3 + count);
			//mesh.TriangleIndices.Add(7 + count);

			//mesh.TriangleIndices.Add(7 + count);
			//mesh.TriangleIndices.Add(3 + count);
			//mesh.TriangleIndices.Add(6 + count);
			//mesh.TriangleIndices.Add(3 + count);
			//mesh.TriangleIndices.Add(2 + count);
			//mesh.TriangleIndices.Add(6 + count);

			//mesh.TriangleIndices.Add(4 + count);
			//mesh.TriangleIndices.Add(5 + count);
			//mesh.TriangleIndices.Add(0 + count);
			//mesh.TriangleIndices.Add(5 + count);
			//mesh.TriangleIndices.Add(1 + count);
			//mesh.TriangleIndices.Add(0 + count);

			//mesh.TriangleIndices.Add(2 + count);
			//mesh.TriangleIndices.Add(1 + count);
			//mesh.TriangleIndices.Add(6 + count);
			//mesh.TriangleIndices.Add(1 + count);
			//mesh.TriangleIndices.Add(5 + count);
			//mesh.TriangleIndices.Add(6 + count);

			//mesh.TriangleIndices.Add(6 + count);
			//mesh.TriangleIndices.Add(5 + count);
			//mesh.TriangleIndices.Add(4 + count);
			//mesh.TriangleIndices.Add(6 + count);
			//mesh.TriangleIndices.Add(4 + count);
			//mesh.TriangleIndices.Add(7 + count);
		}

		#endregion
	}
}
