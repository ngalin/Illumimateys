using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ShadowWall.Api.Repository;

namespace ShadowWall.Api.Controllers
{
    public class KinectController : ApiController
    {
		public IHttpActionResult Get()
		{
			var kinectDepth = new KinectDepth();
			kinectDepth.Rendered += kinectDepth_Rendered;
			return Ok();
		}

		private void kinectDepth_Rendered(object sender, PointEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}
