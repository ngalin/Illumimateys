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

namespace ShadowWall
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
	public partial class Depth : Window
	{
		public Depth()
		{
			InitializeComponent();

			this.Wall = new Wall(KinectCanvas);

			this.Closed += MainWindow_Closed;

			var sensor = KinectSensor.GetDefault();
			sensor.Open();

			var depthReader = sensor.DepthFrameSource.OpenReader();
			depthReader.FrameArrived += depthReader_FrameArrived;
		}

		public Wall Wall { get; set; }

		public int Distance { get { return 2000; } }

		private void MainWindow_Closed(object sender, EventArgs e)
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
						depths[i] = depths[i] > Distance ? default(ushort) : depths[i];
					}

					var pixels = this.ConvertToByteArray(depths, frame.DepthMinReliableDistance, frame.DepthMaxReliableDistance);

					DepthImage.Source = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, BitmapPalettes.WebPalette, pixels, width * PixelFormats.Bgr32.BitsPerPixel / 8);
				}
			}
		}

		private byte[] ConvertToByteArray(ushort[] array, int min = 0, int max = 0)
		{
			var bytes = new byte[array.Length * (PixelFormats.Bgr32.BitsPerPixel / 8)];

			var index = 0;
			for (int i = 0; i < array.Length; ++i)
			{
				var item = array[i];

				var intensity = item; //((float)item / Distance) * 256;

				var intensity1 = intensity / 3;
				var intensity2 = (intensity - intensity1) / 3;
				var intensity3 = (intensity - intensity1 - intensity2) / 3;

				bytes[index++] = (byte)intensity1; // Blue
				bytes[index++] = (byte)intensity2; // Green
				bytes[index++] = (byte)intensity3; // Red
				bytes[index++] = 0; // Alpha
			}

			return bytes;
		}
	}
}
