using IngressCFHostUpdate.KServices.CloudFlare.APIObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IngressCFHostUpdate.KServices.CloudFlare.APIConverters
{
	public class DNSRecordPostConverter : JsonConverter<DNSRecord>
	{
		public override DNSRecord Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
		{
			throw new NotImplementedException();
		}

		public override void Write( Utf8JsonWriter writer, DNSRecord value, JsonSerializerOptions options )
		{
			writer.WriteStartObject();
			writer.WritePropertyName( "type" );
			writer.WriteStringValue( value.Type );
			writer.WritePropertyName( "name" );
			writer.WriteStringValue( value.Name );
			writer.WritePropertyName( "content" );
			writer.WriteStringValue( value.Content );
			writer.WritePropertyName( "ttl" );
			writer.WriteNumberValue( value.TTL );
			writer.WritePropertyName( "proxied" );
			writer.WriteBooleanValue( value.Proxied );
			writer.WriteEndObject();
		}
	}
}
