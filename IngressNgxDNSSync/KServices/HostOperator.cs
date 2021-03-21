using System.Collections.Generic;

namespace IngressNgxDNSSync.KServices
{
	public interface IHostOperator
	{
		bool IsAvailable { get; }
		void Update( string[] addedHosts, string[] removedHosts );
		void Sync();
	}

	abstract public class HostOperator
	{
		public bool DryRun { get; private set; }

		public HostOperator( bool DryRun )
		{
			this.DryRun = DryRun;
		}
	}
}