using System;

namespace ShadowWall
{
	public interface IFrameProvider : IDisposable
	{
		event EventHandler<FrameArrivedArgs> FrameArrived;
	}

	public class FrameArrivedArgs : EventArgs
	{
		public FrameArrivedArgs(ushort[] depths, int width, int height)
		{
			Depths = depths;
			Width = width;
			Height = height;
		}

		public ushort[] Depths { get; }
		public int Width { get; }
		public int Height { get; }
	}
}
