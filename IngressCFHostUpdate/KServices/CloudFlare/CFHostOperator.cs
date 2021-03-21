using IngressCFHostUpdate.KServices.CloudFlare.APIObjects;
using k8s;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngressCFHostUpdate.KServices.CloudFlare
{
	[HostOperator]
	public class CFHostOperator : HostOperator, IHostOperator
	{
		public CFHostOperator( bool DryRun ) : base( DryRun ) { }

		public async void Update( string[] AddedHosts, string[] RemovedHosts )
		{
			string PublicIP = await Ext.GetPublicIP();

			if ( string.IsNullOrEmpty( PublicIP ) )
				return;

			IEnumerable<Zone> Zones = ( await APIClient.List<Zone>() )
				.Items
				.Where( x => AddedHosts.Any( h => h.EndsWith( x.Name ) ) || RemovedHosts.Any( h => h.EndsWith( x.Name ) ) );

			foreach ( Zone Zone in Zones )
			{
				List<DNSRecord> OldRecords = ( await APIClient.List<DNSRecord>( Zone.Id ) )
					.Items
					.Where( x => x.Type == "A" && (
						AddedHosts.Any( x2 => x2.StartsWith( x.Name ) )
						|| RemovedHosts.Any( x2 => x2.StartsWith( x.Name ) )
					) )
					.ToList();

				int ZoneNameLen = Zone.Name.Length;
				foreach ( string Host in AddedHosts )
				{
					DNSRecord ActiveRecord = OldRecords.Where( x => Host.StartsWith( x.Name ) ).FirstOrDefault();
					if ( ActiveRecord == null )
					{
						await APIClient.Create(
							new DNSRecord() { Type = "A", Name = Host.Substring( 0, Host.Length - ZoneNameLen - 1 ), Content = PublicIP }
							, DryRun, Zone.Id
						);
					}
					else
					{
						OldRecords.Remove( ActiveRecord );
					}
				}

				foreach ( DNSRecord OldRecord in OldRecords )
				{
					await APIClient.Delete<DNSRecord>( DryRun, OldRecord.ZoneId, OldRecord.Id );
				}
			}
		}

		public async void Sync()
		{
			string PublicIP = await Ext.GetPublicIP();

			if ( string.IsNullOrEmpty( PublicIP ) )
				return;

			string[] ManagedHosts = ( await Ext.Kube.ListIngressForAllNamespacesAsync() )
				.Items
				.SelectMany( x => x.Spec.Rules.Select( x => x.Host ) )
				.ToArray()
			;

			IEnumerable<Zone> Zones = ( await APIClient.List<Zone>() )
				.Items
				.Where( x => ManagedHosts.Any( h => h.EndsWith( x.Name ) ) );

			foreach ( Zone Zone in Zones )
			{
				List<DNSRecord> OldRecords = ( await APIClient.List<DNSRecord>( Zone.Id ) )
					.Items
					.Where( x => x.Type == "A" && x.Content == PublicIP )
					.ToList();

				int ZoneNameLen = Zone.Name.Length;
				foreach ( string Host in ManagedHosts )
				{
					DNSRecord ActiveRecord = OldRecords.Where( x => Host.StartsWith( x.Name ) ).FirstOrDefault();
					if ( ActiveRecord == null )
					{
						await APIClient.Create(
							new DNSRecord() { Type = "A", Name = Host.Substring( 0, Host.Length - ZoneNameLen - 1 ), Content = PublicIP }
							, false, Zone.Id
						);
					}
					else
					{
						OldRecords.Remove( ActiveRecord );
					}
				}

				foreach ( DNSRecord OldRecord in OldRecords )
				{
					await APIClient.Delete<DNSRecord>( false, OldRecord.ZoneId, OldRecord.Id );
				}
			}
		}

	}
}
