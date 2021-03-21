using IngressCFHostUpdate.KServices.CloudFlare.APIObjects;
using IngressCFHostUpdate.KServices.Kubernetes.WebhookServer.APIObjects;
using k8s;
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
		public static async Task UpdateHosts()
		{
			Kubernetes Kube = new Kubernetes( KubernetesClientConfiguration.BuildDefaultConfig() );

			string LoadBalancerIP = Kube.ListServiceForAllNamespaces().Items.Where( x => x.Spec.Type == "LoadBalancer" ).First().Status.LoadBalancer.Ingress.First().Ip;

			string[] ManagedHosts = ( await Kube.ListIngressForAllNamespacesAsync() )
				.Items
				.SelectMany( x => x.Spec.Rules.Select( x => x.Host ) )
				.ToArray()
			;

			IEnumerable<Zone> Zones = ( await KServices.CloudFlare.APIClient.List<Zone>() )
				.Items
				.Where( x => ManagedHosts.Any( h => h.EndsWith( x.Name ) ) );

			foreach ( Zone Zone in Zones )
			{
				List<DNSRecord> OldRecords = ( await KServices.CloudFlare.APIClient.List<DNSRecord>( Zone.Id ) )
					.Items
					.Where( x => x.Type == "A" && x.Content == LoadBalancerIP )
					.ToList();

				int ZoneNameLen = Zone.Name.Length;
				foreach ( string Host in ManagedHosts )
				{
					DNSRecord ActiveRecord = OldRecords.Where( x => Host.StartsWith( x.Name ) ).FirstOrDefault();
					if ( ActiveRecord == null )
					{
						await KServices.CloudFlare.APIClient.Create(
							new DNSRecord() { Type = "A", Name = Host.Substring( 0, Host.Length - ZoneNameLen - 1 ), Content = LoadBalancerIP }
							, Zone.Id
						);
					}
					else
					{
						OldRecords.Remove( ActiveRecord );
					}
				}

				foreach ( DNSRecord OldRecord in OldRecords )
				{
					await KServices.CloudFlare.APIClient.Delete<DNSRecord>( OldRecord.ZoneId, OldRecord.Id );
				}
			}
		}

		public static void TriggerAdmission( KAdmissionReview AdmissionReview )
		{
			Console.WriteLine( JsonSerializer.Serialize( AdmissionReview ) );
		}

	}
}
