using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace ShadowWall
{
	public class BodyFrame
	{
		private IList<Joint> _joints;

		public IList<Joint> Joints 
		{
			get
			{
				if (_joints == null)
				{
					_joints = new List<Joint>();
				}

				return _joints;
			}
		}
	}
}
