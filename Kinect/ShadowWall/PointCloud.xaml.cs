using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ShadowWall
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class PointCloud : Window, INotifyPropertyChanged
	{
		public PointCloud()
		{
			InitializeComponent();
			
			Closed += PointCloud_Closed;

			//pointCloudWriter = new HttpPointCloudWriter(); // This will transmit the processed point cloud over HTTP
			//pointCloudWriter = new MeshGeometryPointCloudWriter(Mesh); // This will print the processed point cloud on the 3D mesh
			pointCloudWriter = new ImagePointCloudWriter(DepthImage); // This will print the processed point cloud in the Depth image control
			//pointCloudWriter = new SocketCloudWriter(DepthImage);

			//frameProvider = new KinnectFrameProvider();
			frameProvider = new RecordingFrameProvider(@"C:\Users\cgled\Desktop\ShadowWallRecording1.csv");

			frameProvider.FrameArrived += depthReader_FrameArrived;

			filters = new IPointCloudFilter[]
			{
				//new AgingFilter(),
				new GroundingFilter()
			};

			DataContext = this;
		}

		void PointCloud_Closed(object sender, EventArgs e)
		{
			frameProvider.Dispose();
			pointCloudWriter.Dispose();
		}

		void depthReader_FrameArrived(object sender, FrameArrivedArgs e)
		{
			if(last != null && !last.IsCompleted)
			{
				return;
			}
			
			var depths = e.Depths;

			if (IsRecording)
			{
				RecordDepths(depths);
			}

			for (int i = 0; i < depths.Length; ++i)
			{
				depths[i] = depths[i] > (WallBreadth * 10) ? default(ushort) : depths[i];
			}
					
			var newCloud = ConvertToPointCloud(depths, e.Width, e.Height);

			foreach (var filter in filters)
			{
				newCloud = filter.Apply(newCloud);
			}
			Dispatcher.Invoke(() => last = pointCloudWriter.WritePointCloudAsync(AsEnumerable(newCloud), e.Width, e.Height));
			last.Wait();
			Flush();
		}

		Task last;

		IEnumerable<PointFrame> AsEnumerable(PointFrame[,] cloud)
		{
			for (int i = cloud.GetLength(1) -1; i >= 0; --i)
			{
				for (int j = 0; j < cloud.GetLength(0); ++j)
				{
					yield return cloud[j, i];
				}
			}
		}

		void RecordDepths(ushort[] depths)
		{
			var recordString = string.Join(",", depths) + Environment.NewLine;
			var byteCount = Encoding.UTF8.GetByteCount(recordString);
			RecordFileStream.Write(Encoding.UTF8.GetBytes(recordString), 0, byteCount);
		}

		PointFrame[,] ConvertToPointCloud(ushort[] depths, int width, int height)
		{
			var points = PointFrame.NewCloud(width, height);

			for (int i = 0; i < depths.Length; ++i)
			{
				var item = depths[i];
				var x = (int)((i % width) * this.WallWidth / (float)width);
				var y = (int)((height - i / width) * this.WallHeight / (float)height);
				var z = item > 0 ? this.WallBreadth - (((float)item / (this.WallBreadth * 10)) * this.WallBreadth) : 0;

				var b = item / 3;
				var g = (item - b) / 3;
				var r = (item - b - g) / 3;

				points[x, y] = new PointFrame() { X = x, Y = y, Z = z, R = (byte)r, G = (byte)g, B = (byte)b };
			}

			return points;
		}

		void recordButton_Click(object sender, EventArgs e)
		{
			if (!IsRecording)
			{
				var recordFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), string.Format("ShadowWallRecording{0}.csv", recordingsMade++));
				RecordFileStream = new FileStream(recordFile, FileMode.Create, FileAccess.Write);
			}
			else
			{
				RecordFileStream.Flush();
				RecordFileStream.Dispose();
			}

			IsRecording = !IsRecording;
		}
		
		void Flush()
		{
			Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public int WallWidth { get { return 180; } }
		public int WallHeight { get { return 120; } }
		public int WallBreadth { get { return 800; } }
		public string RecordButtonContent
		{
			get
			{
				return IsRecording ? "Stop recording" : "Start recording";
			}
		}

		bool IsRecording
		{
			get
			{
				return isRecording;
			}
			set
			{
				isRecording = value;
				PropertyChanged(this, new PropertyChangedEventArgs("RecordButtonContent"));
			}
		}

		#region Recording state

		bool isRecording = false;
		Stream RecordFileStream { get; set; }
		int recordingsMade = 0;

		#endregion

		readonly IPointCloudWriter pointCloudWriter;
		readonly IFrameProvider frameProvider;
		IEnumerable<IPointCloudFilter> filters;
	}
}
