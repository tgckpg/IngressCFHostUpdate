using IngressCFHostUpdate.KServices;
using IngressCFHostUpdate.KServices.CloudFlare.APIObjects;
using IngressCFHostUpdate.KServices.Kubernetes.WebhookServer.APIObjects;
using k8s;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IngressCFHostUpdate
{
	class Operations
	{
		private static IEnumerable<IHostOperator> GetHostOperators( params object[] Args )
		{
			Type IFace = typeof( IHostOperator );
			return AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany( s => s.GetTypes() )
				.Where( x => IFace.IsAssignableFrom( x ) && !x.IsInterface )
				.Select( x => {
					return ( IHostOperator ) Activator.CreateInstance( x, Args );
				} );
		}

		private static JsonSerializerOptions DebugJsonPrint = new() { WriteIndented = true };

		public static void TriggerIngressAdmission( KAdmissionReview<KIngress> AdmissionReview )
		{
			string[] OldHosts = AdmissionReview.Request.OldObject?.Spec?.Rules?.Select( x => x.Host ).ToArray() ?? Array.Empty<string>();
			string[] NewHosts = AdmissionReview.Request.Object?.Spec?.Rules?.Select( x => x.Host ).ToArray() ?? Array.Empty<string>();

			if ( !( OldHosts.Any() || NewHosts.Any() ) )
				return;

			Ext.GetLogger<Operations>().LogInformation( JsonSerializer.Serialize( AdmissionReview, DebugJsonPrint ) );

			string[] RemovedHosts = OldHosts.Except( NewHosts ).ToArray();
			string[] AddedHosts = NewHosts.Except( OldHosts ).ToArray();

			foreach ( IHostOperator Op in GetHostOperators( AdmissionReview.Request.DryRun ) )
			{
				Op.Update( AddedHosts, RemovedHosts );
			}
		}

	}
}
