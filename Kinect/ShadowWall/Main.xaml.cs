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
	public partial class Main : Window
	{
		public Main()
		{
			InitializeComponent();

			this.Wall = new Wall(KinectCanvas);

			this.Closed += MainWindow_Closed;

			var sensor = KinectSensor.GetDefault();
			sensor.Open();

			var bodyReader = sensor.BodyFrameSource.OpenReader();
			bodyReader.FrameArrived += bodyReader_FrameArrived;

			var colorReader = sensor.ColorFrameSource.OpenReader();
			colorReader.FrameArrived += colorReader_FrameArrived;

			var depthReader = sensor.DepthFrameSource.OpenReader();
			depthReader.FrameArrived += depthReader_FrameArrived;

			var infraredReader = sensor.InfraredFrameSource.OpenReader();
			infraredReader.FrameArrived += infraredReader_FrameArrived;
		}

		public Wall Wall { get; set; }

		private void MainWindow_Closed(object sender, EventArgs e)
		{
			KinectSensor.GetDefault().Close();
		}

		private void bodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
		{
			using (var frame = e.FrameReference.AcquireFrame())
			{
				if (frame != null)
				{
					Wall.Clear();

					var bodies = new Body[frame.BodyCount];
					frame.GetAndRefreshBodyData(bodies);

					foreach (var body in bodies.Where(b => b.IsTracked))
					{
						foreach (var joint in body.Joints.Select(j => j.Value))
						{
							Wall.DrawPoint(joint, Brushes.White);
						}

						//Serializer.Save(body);
					}
				}
			}
		}

		private void colorReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
		{
			using (var frame = e.FrameReference.AcquireFrame())
			{
				if (frame != null)
				{
					var width = frame.FrameDescription.Width;
					var height = frame.FrameDescription.Height;
					var pixels = new byte[width * height * (PixelFormats.Bgr32.BitsPerPixel / 8)];

					frame.CopyConvertedFrameDataToArray(pixels, ColorImageFormat.Bgra);

					ColorImage.Source = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, BitmapPalettes.WebPalette, pixels, width * PixelFormats.Bgr32.BitsPerPixel / 8);
				}
			}
		}

		private void depthReader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
		{
			using (var frame = e.FrameReference.AcquireFrame())
			{
				if (frame != null)
				{
					var width = frame.FrameDescription.Width;
					var height = frame.FrameDescription.Height;
					var depth = new ushort[width * height];

					frame.CopyFrameDataToArray(depth);
					var pixels = this.ConvertToByteArray(depth, frame.DepthMinReliableDistance, frame.DepthMaxReliableDistance);

					DepthImage.Source = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, BitmapPalettes.WebPalette, pixels, width * PixelFormats.Bgr32.BitsPerPixel / 8);
				}
			}
		}

		private void infraredReader_FrameArrived(object sender, InfraredFrameArrivedEventArgs e)
		{
			using (var frame = e.FrameReference.AcquireFrame())
			{
				if (frame != null)
				{
					var width = frame.FrameDescription.Width;
					var height = frame.FrameDescription.Height;
					var infrared = new ushort[width * height];

					frame.CopyFrameDataToArray(infrared);
					var pixels = this.ConvertToByteArray(infrared);

					InfraredImage.Source = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, BitmapPalettes.Halftone256, pixels, width * PixelFormats.Bgr32.BitsPerPixel / 8);
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

				var intensity = (byte)(item >> 8);

				if(min > 0 && max > 0)
				{
					intensity = (byte)(item >= min && item <= max ? item : 0);
				}

				bytes[index++] = intensity; // Blue
				bytes[index++] = intensity; // Green   
				bytes[index++] = intensity; // Red

				++index;
			}

			return bytes;
		}
	}
}
