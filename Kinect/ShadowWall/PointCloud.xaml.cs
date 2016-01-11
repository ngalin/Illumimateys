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

		public int WallWidth { get { return 180; } }
		public int WallHeight { get { return 120; } }
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

					this.Clear();
					this.ConvertToPointCloud(depths, width, height);
					this.Flush();
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
					var x = (i % width) * this.WallWidth / (float)width;
					var y = (height - i / width) * this.WallHeight / (float)height;
					var z = item > 0 ? this.WallWidth - (((float)item / this.Distance) * this.WallWidth) : 0;

					var b = item / 3;
					var g = (item - b) / 3;
					var r = (item - b - g) / 3;

					this.DrawPoint(x, y, z, r, g, b);
					//Serializer.Save(x, y, z, r, g, b);
				}
			}
		}

		private void Clear()
		{
			Mesh.Positions.Clear();
			Mesh.TriangleIndices.Clear();
		}

		private void Flush()
		{
			Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
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
					Camera.Position = new Point3D(Camera.Position.X - 50, Camera.Position.Y - 50, Camera.Position.Z);
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

		private void DrawPoint(float x, float y, float z, int r, int g, int b)
		{
			var count = Mesh.Positions.Count;
			var color = new Color() { A = 0, R = (byte)r, G = (byte)g, B = (byte)b };

			Mesh.Positions.Add(new Point3D(x - 0.5, y - 0.5, z + 0.5));
			Mesh.Positions.Add(new Point3D(x + 0.5, y + 0.5, z + 0.5));
			Mesh.Positions.Add(new Point3D(x - 0.5, y + 0.5, z + 0.5));

			Mesh.TriangleIndices.Add(0 + count);
			Mesh.TriangleIndices.Add(1 + count);
			Mesh.TriangleIndices.Add(2 + count);
		}

		#endregion
	}
}
