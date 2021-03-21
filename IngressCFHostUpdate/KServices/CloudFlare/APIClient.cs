using IngressCFHostUpdate.KServices.CloudFlare.APIObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IngressCFHostUpdate.KServices.CloudFlare
{
	public class APIClient
	{
		private static string APIToken = Environment.GetEnvironmentVariable( "CF_API_TOKEN" );
		const string ServiceURI = "https://api.cloudflare.com/client/v4";

		public static async Task<ListResult<T>> List<T>( params string[] Args )
		{
			string Url = ServiceURI + "/" + typeof( T ).GetApiEndPoint().List;
			if ( Args.Any() )
			{
				Url = string.Format( Url, Args );
			}

			WebRequest Request = WebRequest.Create( Url );
			Request.Headers.Add( HttpRequestHeader.Authorization, $"Bearer {APIToken}" );
			using var Response = await Request.GetResponseAsync();
			return await JsonSerializer.DeserializeAsync<ListResult<T>>( Response.GetResponseStream() );
		}

		public static async Task<Result<T>> Create<T>( T APIObject, params string[] Args )
		{
			string Url = ServiceURI + "/" + typeof( T ).GetApiEndPoint().Create;
			if ( Args.Any() )
			{
				Url = string.Format( Url, Args );
			}

			WebRequest Request = WebRequest.Create( Url );
			Request.Headers.Add( HttpRequestHeader.Authorization, $"Bearer {APIToken}" );
			Request.Headers.Add( HttpRequestHeader.Accept, "application/json" );
			Request.Method = WebRequestMethods.Http.Post;
			Request.ContentType = "application/json";

			JsonConverter Converter = typeof( T ).GetApiPostConverter()?.Create;

			using var ReqStream = Request.GetRequestStream();
			if ( Converter != null )
			{
				JsonSerializerOptions Options = new JsonSerializerOptions();
				Options.Converters.Add( Converter );
				string A = JsonSerializer.Serialize( APIObject, Options );
				await JsonSerializer.SerializeAsync( ReqStream, APIObject, Options );
			}
			else
			{
				await JsonSerializer.SerializeAsync( ReqStream, APIObject );
			}

			WebResponse Response = await Request.GetResponseAsync();

			using var RespStream = Response.GetResponseStream();
			return await JsonSerializer.DeserializeAsync<Result<T>>( RespStream );
		}

		public static async Task<DeleteResult> Delete<T>( params string[] Args )
		{
			string Url = ServiceURI + "/" + typeof( T ).GetApiEndPoint().Delete;
			if ( Args.Any() )
			{
				Url = string.Format( Url, Args );
			}

			// LogDelete<T>( Args );

			WebRequest Request = WebRequest.Create( Url );
			Request.Headers.Add( HttpRequestHeader.Authorization, $"Bearer {APIToken}" );
			Request.Headers.Add( HttpRequestHeader.Accept, "application/json" );
			Request.Method = "DELETE";
			Request.ContentType = "application/json";

			WebResponse Response = await Request.GetResponseAsync();
			using var RespStream = Response.GetResponseStream();
			return await JsonSerializer.DeserializeAsync<DeleteResult>( RespStream );
		}

	}
}
