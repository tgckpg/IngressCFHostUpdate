using IngressNgxDNSSync.KServices.Kubernetes.WebhookServer.APIObjects;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngressNgxDNSSync.KServices.Kubernetes.WebhookServer.Controllers
{
	[ApiController]
	public class AdmissionController : Controller
	{
		[HttpGet]
		[Route( "" )]
		public JsonResult ProbeEndpoint() => new JsonResult( new { } );

		[HttpGet]
		[Route( "/sync" )]
		public JsonResult Sync()
		{
			Operations.TriggerSync( false );
			return new JsonResult( new { status = true, message = "OK" } );
		}

		[HttpPost]
		[Route( "/operation-log/json-objects" )]
		public JsonResult Sync( [FromForm] bool Enable )
		{
			Operations.LogJsonObjects = Enable;
			return new JsonResult( new { status = true, message = "OK" } );
		}

		[HttpGet]
		[Route( "/test-sync" )]
		public JsonResult TestSync()
		{
			Operations.TriggerSync( true );
			return new JsonResult( new { status = true, message = "OK" } );
		}

		[HttpPost]
		[Route( "/validating-webhook" )]
		public JsonResult ValidatingWebhook( [FromBody] KAdmissionReview<KIngress> AdmissionReview )
		{
			Operations.TriggerIngressAdmission( AdmissionReview );

			// Always returns true because we are only inspecting for ingress hosts
			return new JsonResult( new KAdmissionResponse()
			{
				Response = new KResponse() { UID = AdmissionReview.Request.UID }
			} );
		}
	}
}
