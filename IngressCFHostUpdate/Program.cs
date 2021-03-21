using IngressCFHostUpdate.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
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
		internal static IWebHost Host;

		static void Main( string[] args )
		{
			Host = new WebHostBuilder()
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
				.ConfigureLogging( options =>
				{
					// You'll get the following mystic error if to use addConsole, addDebug, addEventLog, etc.:
					//    AuthenticationException: The remote certificate is invalid because of errors in the certificate chain: PartialChain
					options.ClearProviders()
						.AddProvider( new ColorConsoleLoggerProvider( new ColorConsoleLoggerConfiguration() ) )
						.AddProvider( new ColorConsoleLoggerProvider( new ColorConsoleLoggerConfiguration() { LogLevel = LogLevel.Error, Color = ConsoleColor.Red } ) )
						.AddProvider( new ColorConsoleLoggerProvider( new ColorConsoleLoggerConfiguration() { LogLevel = LogLevel.Warning, Color = ConsoleColor.Yellow } ) )
					;
				} )
				.UseStartup<KServices.Kubernetes.WebhookServer.Startup>()
				.Build();

			Host.Run();
		}
	}
}
