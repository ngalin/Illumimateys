using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace ShadowWall
{
	public class Wall
	{
		public Wall(Canvas canvas)
		{
			this.Canvas = canvas;
		}

		public Canvas Canvas { get; set; }

		public void Clear()
		{
			this.Canvas.Children.Clear();
		}

		public void DrawPoint(Joint joint)
		{
			var thickness = 5.0;
			var x = Scale.X(joint.Position.X, this.Canvas.ActualWidth);
			var y = Scale.Y(joint.Position.Y, this.Canvas.ActualHeight);

			var line = new Line
			{
				X1 = x,
				Y1 = y,
				X2 = x + thickness,
				Y2 = y + thickness,
				StrokeThickness = thickness,
				Stroke = Brushes.White
			};

			this.Canvas.Children.Add(line);
		}
	}
}
