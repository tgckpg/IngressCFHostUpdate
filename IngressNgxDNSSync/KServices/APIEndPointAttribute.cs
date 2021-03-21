using System;

namespace IngressNgxDNSSync.KServices
{
	[AttributeUsage( AttributeTargets.Class )]
	public class APIEndPointAttribute : Attribute
	{
		public string List { get; set; }
		public string Get { get; set; }
		public string Create { get; set; }
		public string Delete { get; set; }

		public APIEndPointAttribute( string List = null, string Get = null, string Create = null, string Delete = null )
		{
			this.List = List;
			this.Get = Get;
			this.Create = Create;
			this.Delete = Delete;
		}
	}
}
