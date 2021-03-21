using System;
using System.Text.Json.Serialization;

namespace IngressCFHostUpdate.KServices
{
	[AttributeUsage( AttributeTargets.Class )]
	public class APIPostConverterAttribute : Attribute
	{
		public JsonConverter Create => ( JsonConverter ) Activator.CreateInstance( _Create );

		private Type _Create { get; set; }

		public APIPostConverterAttribute( Type Create = null )
		{
			this._Create = Create;
		}

	}
}
