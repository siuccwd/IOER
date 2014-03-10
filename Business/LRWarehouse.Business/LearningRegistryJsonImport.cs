using System;
using System.Collections.Generic;
using System.Text;

namespace LRWarehouse.Business
{
	/// <summary>
	/// Represents an object that describes a LearningRegistryJsonImport
	/// </summary>
	public class LearningRegistryJsonImport
	{

		public class RootObject
		{
			public List<Document> documents { get; set; }
			public string resumption_token { get; set; }
		}

		public class TOS
		{
			public string submission_attribution { get; set; }
			public string submission_TOS { get; set; }
		}

		public class DigitalSignature
		{
			public List<string> key_location { get; set; }
			public string signing_method { get; set; }
			public string signature { get; set; }
			public string key_owner { get; set; }
		}

		public class Identity
		{
			public string signer { get; set; }
			public string submitter { get; set; }
			public string submitter_type { get; set; }
			public string curator { get; set; }
			public string owner { get; set; }
		}

		public class Document2
		{
			public string doc_type { get; set; }
			public string resource_locator { get; set; }
			public object resource_data { get; set; }
			public string update_timestamp { get; set; }
			public TOS TOS { get; set; }
			public string _rev { get; set; }
			public string resource_data_type { get; set; }
			public string payload_schema_locator { get; set; }
			public string payload_placement { get; set; }
			public List<string> payload_schema { get; set; }
			public string node_timestamp { get; set; }
			public DigitalSignature digital_signature { get; set; }
			public string create_timestamp { get; set; }
			public string doc_version { get; set; }
			public bool active { get; set; }
			public string publishing_node { get; set; }
			public string _id { get; set; }
			public string doc_ID { get; set; }
			public Identity identity { get; set; }
			public List<object> keys { get; set; }
			public int? weight { get; set; }
			public int? resource_TTL { get; set; }
		}

		public class Document
		{
			public List<Document2> document { get; set; }
			public string doc_ID { get; set; }
		}

	}

}
