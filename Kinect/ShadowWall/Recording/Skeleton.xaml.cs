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
	public partial class Skeleton : Window
	{
		public Skeleton()
		{
			InitializeComponent();

			this.Wall = new Wall(KinectCanvas);

			this.Loaded += Recording_Loaded;
		}

		public Wall Wall { get; set; }

		private void Recording_Loaded(object sender, RoutedEventArgs e)
		{
			var bodies = new Serializer().LoadSkeleton();

			foreach (var body in bodies)
			{
				Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
				{
					Wall.Clear();

					foreach (var joint in body.Joints)
					{
						Wall.DrawPoint(joint, Brushes.White);
					}

					Thread.Sleep(30);
				}));
			}
		}
	}
}
