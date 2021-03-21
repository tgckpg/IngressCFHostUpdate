using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace IngressCFHostUpdate
{
	class Program
	{
#if DEBUG
		const int HTTPS_PORT = 8443;
#else
		const int HTTPS_PORT = 443;
#endif
		static void Main( string[] args )
		{
			IWebHost Host = WebHost.CreateDefaultBuilder()
				.UseKestrel( options =>
				{
					options.Listen( IPAddress.Any, HTTPS_PORT, listenOptions =>
					{
						// See: https://github.com/dotnet/runtime/issues/23749#issuecomment-388231655
						var PEMCert = X509Certificate2.CreateFromPemFile(
							Environment.GetEnvironmentVariable( "CA_CERT" )
							, Environment.GetEnvironmentVariable( "CA_KEY" )
						);
						listenOptions.UseHttps( new X509Certificate2( PEMCert.Export( X509ContentType.Pkcs12 ) ) );
					} );
				} )
				.UseStartup<KServices.Kubernetes.WebhookServer.Startup>()
				.Build();

			Host.Run();
		}
	}
}
