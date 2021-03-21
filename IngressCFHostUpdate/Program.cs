using System;
using System.Threading.Tasks;

namespace IngressCFHostUpdate
{
	class Program
	{
		static async Task Main( string[] args )
		{
			Console.WriteLine( "Hello World!" );
			await Operations.UpdateHosts();
		}
	}
}
