using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IngressCFHostUpdate.KServices
{

	public static class Ext
	{
		public static APIEndPointAttribute GetApiEndPoint( this Type APIObjectClass )
		{
			return APIObjectClass.GetCustomAttributes()
				.Where( x => x is APIEndPointAttribute )
				.Cast<APIEndPointAttribute>()
				.First();
		}

		public static APIPostConverterAttribute GetApiPostConverter( this Type APIObjectClass )
		{
			return APIObjectClass.GetCustomAttributes()
				.Where( x => x is APIPostConverterAttribute )
				.Cast<APIPostConverterAttribute>()
				.FirstOrDefault();
		}
	}

}
