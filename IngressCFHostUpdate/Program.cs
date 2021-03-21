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
		const int HTTP_PORT = 8080;
#else
		const int HTTPS_PORT = 443;
		const int HTTP_PORT = 80;
#endif
		static void Main( string[] args )
		{
			IWebHost Host = WebHost.CreateDefaultBuilder()
				.UseKestrel( options =>
				{
					// NOTE: Admissions Controller API must be in https
					// See: https://kubernetes.io/docs/reference/access-authn-authz/extensible-admission-controllers/#url
					string CACertPath = Environment.GetEnvironmentVariable( "CA_CERT" );
					string CAKeyPath = Environment.GetEnvironmentVariable( "CA_KEY" );

					// We need to import the cert in pem format and use it in pkcs format
					// See: https://github.com/dotnet/runtime/issues/23749#issuecomment-388231655
					var PEMCert = X509Certificate2.CreateFromPemFile( CACertPath, CAKeyPath );
					options.Listen( IPAddress.Any, HTTPS_PORT, listenOptions =>
					{
						listenOptions.UseHttps( new X509Certificate2( PEMCert.Export( X509ContentType.Pkcs12 ) ) );
					} );
				} )
				.UseStartup<KServices.Kubernetes.WebhookServer.Startup>()
				.Build();

			Host.Run();
		}
	}
}
