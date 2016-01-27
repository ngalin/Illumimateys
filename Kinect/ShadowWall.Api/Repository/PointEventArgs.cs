using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShadowWall.Api.Repository
{
	public delegate void PointEventHandler(object sender, PointEventArgs e);

	public class PointEventArgs : EventArgs
	{
		public List<PointFrame> Points { get; set; }
	}
}
