using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ShadowWall.Api.Repository;

namespace ShadowWall.Api.Controllers
{
    public class KinectController : ApiController
    {
		public IHttpActionResult Get()
		{
			var points = new List<PointFrame>();
			var kinectDepth = new KinectDepth();
			//kinectDepth.Rendered = data => points = data.ToList();

			//var task = Request.Content.ReadAsMultipartAsync().ContinueWith<IEnumerable<PointFrame>>(t => 
			//{
			//	IEnumerable<PointFrame> points;
			//	//kinectDepth.Rendered = await lambda;
			//	return points;
			//});

			return Ok(kinectDepth.GetPoints());
		}
	}
}
