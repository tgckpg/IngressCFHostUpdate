using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IngressNgxDNSSync.KServices.CloudFlare.APIObjects
{
	public class ListResult<T>
	{
		[JsonPropertyName( "success" )]
		public bool Success { get; set; }
		[JsonPropertyName( "errors" )]
		public string[] Errors { get; set; }
		[JsonPropertyName( "messages" )]
		public string[] Messages { get; set; }
		[JsonPropertyName( "result" )]
		public T[] Items { get; set; }
	}

	public class Result<T>
	{
		[JsonPropertyName( "success" )]
		public bool Success { get; set; }
		[JsonPropertyName( "errors" )]
		public string[] Errors { get; set; }
		[JsonPropertyName( "messages" )]
		public string[] Messages { get; set; }
		[JsonPropertyName( "result" )]
		public T Item { get; set; }
	}

	public class DeletedRecord
	{
		[JsonPropertyName( "id" )]
		public string Id { get; set; }
	}

	public class DeleteResult
	{
		[JsonPropertyName( "result" )]
		public DeletedRecord Item { get; set; }
	}

	public class Owner
	{
		[JsonPropertyName( "id" )]
		public string Id { get; set; }
		[JsonPropertyName( "email" )]
		public string Email { get; set; }
		[JsonPropertyName( "type" )]
		public string Type { get; set; }
	}

	public class Account
	{
		[JsonPropertyName( "id" )]
		public string Id { get; set; }
		[JsonPropertyName( "name" )]
		public string Name { get; set; }
	}

	public class Plan
	{
		[JsonPropertyName( "id" )]
		public string Id { get; set; }
		[JsonPropertyName( "name" )]
		public string Name { get; set; }
		[JsonPropertyName( "price" )]
		public double Price { get; set; }
		[JsonPropertyName( "currency" )]
		public string Currency { get; set; }
		[JsonPropertyName( "frequency" )]
		public string Frequency { get; set; }
		[JsonPropertyName( "legacy_id" )]
		public string LegacyId { get; set; }
		[JsonPropertyName( "is_subscribed" )]
		public bool IsSubscribed { get; set; }
		[JsonPropertyName( "can_subscribe" )]
		public bool CanSubscribe { get; set; }

	}

	[APIEndPoint( List: "zones", Get: "zone/{0}" )]
	public class Zone
	{
		[JsonPropertyName( "id" )]
		public string Id { get; set; }
		[JsonPropertyName( "name" )]
		public string Name { get; set; }
		[JsonPropertyName( "development_mode" )]
		public int DevelopmentMode { get; set; }
		[JsonPropertyName( "original_name_servers" )]
		public string[] OriginalNameServers { get; set; }
		[JsonPropertyName( "original_registrar" )]
		public string OriginalRegistrar { get; set; }
		[JsonPropertyName( "original_dnshost" )]
		public string OriginalDNSHost { get; set; }
		[JsonPropertyName( "created_on" )]
		public DateTime DateCreated { get; set; }
		[JsonPropertyName( "modified_on" )]
		public DateTime DateModified { get; set; }
		[JsonPropertyName( "activated_on" )]
		public DateTime DateActivated { get; set; }
		[JsonPropertyName( "owner" )]
		public Owner Owner { get; set; }
		[JsonPropertyName( "account" )]
		public Account Account { get; set; }
		[JsonPropertyName( "permissions" )]
		public string[] Permissions { get; set; }
		[JsonPropertyName( "plan" )]
		public Plan Plan { get; set; }
		[JsonPropertyName( "plan_pending" )]
		public Plan PlanPending { get; set; }
		[JsonPropertyName( "status" )]
		public string Status { get; set; }
		[JsonPropertyName( "paused" )]
		public bool Paused { get; set; }
		[JsonPropertyName( "type" )]
		public string Type { get; set; }
		[JsonPropertyName( "name_servers" )]
		public string[] NameServers { get; set; }
	}

	[APIPostConverter( Create: typeof( APIConverters.DNSRecordPostConverter ) )]
	[APIEndPoint(
		List: "zones/{0}/dns_records"
		, Get: "zones/{0}/dns_records/{1}"
		, Create: "zones/{0}/dns_records"
		, Delete: "zones/{0}/dns_records/{1}" )]
	public class DNSRecord
	{
		[JsonPropertyName( "id" )]
		public string Id { get; set; }
		[JsonPropertyName( "type" )]
		public string Type { get; set; }
		[JsonPropertyName( "name" )]
		public string Name { get; set; }
		[JsonPropertyName( "content" )]
		public string Content { get; set; }
		[JsonPropertyName( "proxiable" )]
		public bool Proxiable { get; set; }
		[JsonPropertyName( "proxied" )]
		public bool Proxied { get; set; } = true;
		[JsonPropertyName( "ttl" )]
		public int TTL { get; set; } = 1;
		[JsonPropertyName( "locked" )]
		public bool Locked { get; set; }
		[JsonPropertyName( "zone_id" )]
		public string ZoneId { get; set; }
		[JsonPropertyName( "zone_name" )]
		public string ZoneName { get; set; }
		[JsonPropertyName( "created_on" )]
		public DateTime DateCreated { get; set; }
		[JsonPropertyName( "modified_on" )]
		public DateTime DateModified { get; set; }
		[JsonPropertyName( "data" )]
		public Dictionary<string, string> Data { get; set; }
		[JsonPropertyName( "meta" )]
		public Meta Meta { get; set; }
	}

	public class Meta
	{
		[JsonPropertyName( "auto_added" )]
		public bool AutoAdded { get; set; }
		[JsonPropertyName( "managed_by_apps" )]
		public bool ManagedByApps { get; set; }
		[JsonPropertyName( "managed_by_argo_tunnel" )]
		public bool ManagedByArgoTunnel { get; set; }
		[JsonPropertyName( "source" )]
		public string Source { get; set; }
	}
}
