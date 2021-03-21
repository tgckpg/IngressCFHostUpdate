using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IngressCFHostUpdate.KServices.Kubernetes.WebhookServer.APIObjects
{
	public class KUserInfo
	{
		[JsonPropertyName( "username" )]
		public string Username { get; set; }
		[JsonPropertyName( "uid" )]
		public string UID { get; set; }
		[JsonPropertyName( "groups" )]
		public string[] Groups { get; set; }
		[JsonPropertyName( "extra" )]
		public Dictionary<string, string[]> Extra { get; set; }
	}

	public class KKind
	{
		[JsonPropertyName( "group" )]
		public string Group { get; set; }
		[JsonPropertyName( "version" )]
		public string version { get; set; }
		[JsonPropertyName( "kind" )]
		public string Kind { get; set; }
	}

	public class KResqurce
	{
		[JsonPropertyName( "group" )]
		public string Group { get; set; }
		[JsonPropertyName( "version" )]
		public string version { get; set; }
		[JsonPropertyName( "resource" )]
		public string Resource { get; set; }
	}

	public class KRequest<T>
	{
		[JsonPropertyName( "uid" )]
		public string UID { get; set; }
		[JsonPropertyName( "kind" )]
		public KKind Kind { get; set; }
		[JsonPropertyName( "resource" )]
		public KResqurce Resource { get; set; }
		[JsonPropertyName( "subresource" )]
		public string SubResource { get; set; }

		[JsonPropertyName( "requestKind" )]
		public KKind RequestKind { get; set; }
		[JsonPropertyName( "requestResource" )]
		public KResqurce RequestResource { get; set; }
		[JsonPropertyName( "requestSubResource" )]
		public string RequestSubResource { get; set; }

		[JsonPropertyName( "name" )]
		public string Name { get; set; }
		[JsonPropertyName( "namespace" )]
		public string Namespace { get; set; }

		[JsonPropertyName( "operation" )]
		public string Operation { get; set; }

		[JsonPropertyName( "userInfo" )]
		public KUserInfo UserInfo { get; set; }

		[JsonPropertyName( "object" )]
		public T Object { get; set; }
		[JsonPropertyName( "oldObject" )]
		public T OldObject { get; set; }
		[JsonPropertyName( "options" )]
		public object Options { get; set; }

		[JsonPropertyName( "dryRun" )]
		public bool DryRun { get; set; }
	}

	public class KAdmissionReview<T>
	{
		[JsonPropertyName( "apiVersion" )]
		public string APIVersion { get; set; }
		[JsonPropertyName( "kind" )]
		public string Kind { get; set; }

		[JsonPropertyName( "request" )]
		public KRequest<T> Request { get; set; }

	}

	public class KResponse
	{
		[JsonPropertyName( "uid" )]
		public string UID { get; set; }
		[JsonPropertyName( "allowed" )]
		public bool Allowed { get; set; } = true;
	}

	public class KIngRule
	{
		[JsonPropertyName( "host" )]
		public string Host { get; set; }

		[JsonPropertyName( "http" )]
		public object Http { get; set; }
	}

	public class KIngSpec
	{
		[JsonPropertyName( "rules" )]
		public KIngRule[] Rules { get; set; }
	}

	public class KIngress
	{
		[JsonPropertyName( "apiVersion" )]
		public string APIVersion { get; set; }
		[JsonPropertyName( "kind" )]
		public string Kind { get; set; }

		[JsonPropertyName( "metadata" )]
		public object Metadata { get; set; }

		[JsonPropertyName( "spec" )]
		public KIngSpec Spec { get; set; }

		[JsonPropertyName( "status" )]
		public object Status { get; set; }
	}

	public class KAdmissionResponse
	{
		[JsonPropertyName( "apiVersion" )]
		public string APIVersion { get; set; } = "admission.k8s.io/v1";
		[JsonPropertyName( "kind" )]
		public string Kind { get; set; } = "AdmissionReview";

		[JsonPropertyName( "response" )]
		public KResponse Response { get; set; }

	}
}
