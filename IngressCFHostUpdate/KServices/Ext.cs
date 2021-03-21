using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace IngressCFHostUpdate.KServices
{
	public static class Ext
	{
		private static k8s.Kubernetes _Kube;
		public static k8s.Kubernetes Kube => _Kube ??= new( KubernetesClientConfiguration.BuildDefaultConfig() );

		public static APIEndPointAttribute GetApiEndPoint( this Type APIObjectClass )
			=> APIObjectClass.GetCustomAttributes()
				.Where( x => x is APIEndPointAttribute )
				.Cast<APIEndPointAttribute>()
				.First();

		public static APIPostConverterAttribute GetApiPostConverter( this Type APIObjectClass )
			=> APIObjectClass.GetCustomAttributes()
				.Where( x => x is APIPostConverterAttribute )
				.Cast<APIPostConverterAttribute>()
				.FirstOrDefault();

		public async static Task<string> GetPublicIP()
		{
			string Addr = Environment.GetEnvironmentVariable( "MONITOR_IP" );
			if ( !string.IsNullOrEmpty( Addr ) )
			{
				return Addr;
			}

			try
			{
				Addr = ( await Kube.ListServiceForAllNamespacesAsync() )
					.Items
					.Where( x => x.Spec.Type == "LoadBalancer" )
					.FirstOrDefault()
					?.Status.LoadBalancer.Ingress
					.First().Ip;
			}
			catch( Exception Ex )
			{
				GetLogger<Program>().LogError( Ex, Ex.Message );
			}

			if ( string.IsNullOrEmpty( Addr ) )
				GetLogger<Program>().LogWarning( "Failed to determine the public address" );

			return Addr;
		}

		public static ILogger<T> GetLogger<T>()
		{
			using var serviceScope = Program.Host.Services.CreateScope();
			var services = serviceScope.ServiceProvider;
			return services.GetRequiredService<ILogger<T>>();
		}
	}
}
