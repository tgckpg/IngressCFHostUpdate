using IngressCFHostUpdate.KServices.Kubernetes.WebhookServer.APIObjects;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngressCFHostUpdate.KServices.Kubernetes.WebhookServer.Controllers
{
	[ApiController]
	public class AdmissionController : Controller
	{
		[HttpPost]
		[Route( "/validating-webhook" )]
		public JsonResult ValidatingWebhook( [FromBody] KAdmissionReview AdmissionReview )
		{
			return new JsonResult( new KAdmissionResponse()
			{
				Response = new KResponse() { UID = AdmissionReview.Request.UID }
			} );
		}
	}
}
