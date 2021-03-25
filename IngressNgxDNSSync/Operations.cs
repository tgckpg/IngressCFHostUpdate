using IngressNgxDNSSync.KServices;
using IngressNgxDNSSync.KServices.Kubernetes.WebhookServer.APIObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace IngressNgxDNSSync
{
	class Operations
	{
		private static IEnumerable<IHostOperator> GetHostOperators( params object[] Args )
		{
			Type IFace = typeof( IHostOperator );
			return AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany( s => s.GetTypes() )
				.Where( x => IFace.IsAssignableFrom( x ) && !x.IsInterface )
				.Select( x => ( IHostOperator ) Activator.CreateInstance( x, Args ) )
				.Where( x => x.IsAvailable );
		}

		private static JsonSerializerOptions DebugJsonPrint = new() { WriteIndented = true };

		public static void TriggerIngressAdmission( KAdmissionReview<KIngress> AdmissionReview )
		{
			Ext.GetLogger<Operations>().LogInformation( "Trigger Admission" );

			string[] OldHosts = AdmissionReview.Request.OldObject?.Spec?.Rules?.Select( x => x.Host ).ToArray() ?? Array.Empty<string>();
			string[] NewHosts = AdmissionReview.Request.Object?.Spec?.Rules?.Select( x => x.Host ).ToArray() ?? Array.Empty<string>();

			if ( !( OldHosts.Any() || NewHosts.Any() ) )
				return;

			Ext.GetLogger<Operations>().LogInformation( JsonSerializer.Serialize( AdmissionReview, DebugJsonPrint ) );

			string[] RemovedHosts = OldHosts.Except( NewHosts ).ToArray();
			string[] AddedHosts = NewHosts.Except( OldHosts ).ToArray();

			IHostOperator[] Operators = GetHostOperators( AdmissionReview.Request.DryRun ).ToArray();
			if ( !Operators.Any() )
				Ext.GetLogger<Operations>().LogError( "No available operators" );

			foreach ( IHostOperator Operator in Operators )
			{
				Operator.Update( AddedHosts, RemovedHosts );
			}
		}

		public static void TriggerSync( bool DryRun )
		{
			Ext.GetLogger<Operations>().LogInformation( "Trigger Sync" );

			IHostOperator[] Operators = GetHostOperators( DryRun ).ToArray();
			if ( !Operators.Any() )
				Ext.GetLogger<Operations>().LogError( "No available operators" );

			foreach ( IHostOperator Operator in Operators )
			{
				Operator.Sync();
			}
		}

	}
}
