using System;
using Newtonsoft.Json;

namespace Chatbot.Definitions.Models
{
	public class AttachmentButton
	{
		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("value")]
		public string Value { get; set; }
	}
}
