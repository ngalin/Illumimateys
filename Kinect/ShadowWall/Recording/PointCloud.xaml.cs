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

namespace ShadowWall.Recording
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
			this.KeyDown += PointCloud_KeyDown;
			this.MouseWheel += PointCloud_MouseWheel;
		}

		private void PointCloud_Loaded(object sender, RoutedEventArgs e)
		{
			var points = Serializer.LoadPoint();

			foreach (var point in points)
			{
				this.DrawPoint(point.X, point.Y, point.Z, point.R, point.G, point.B);
			}
		}

		#region 3D

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
			var count = 0;
			var color = new Color() { A = (byte)255, R = (byte)r, G = (byte)g, B = (byte)b };

			var geometry = new MeshGeometry3D();

			geometry.Positions.Add(new Point3D(x - 0.5, y - 0.5, z + 0.5));
			geometry.Positions.Add(new Point3D(x + 0.5, y + 0.5, z + 0.5));
			geometry.Positions.Add(new Point3D(x - 0.5, y + 0.5, z + 0.5));

			geometry.TriangleIndices.Add(0 + count);
			geometry.TriangleIndices.Add(1 + count);
			geometry.TriangleIndices.Add(2 + count);

			ModelGroup.Children.Add(new GeometryModel3D(geometry, new DiffuseMaterial(new SolidColorBrush(color))) { Transform = GeometryModel.Transform });
			this.Flush();
		}

		private void Flush()
		{
			Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
		}
		
		#endregion
	}
}
