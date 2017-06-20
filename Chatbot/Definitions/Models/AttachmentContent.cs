using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chatbot.Definitions.Models
{
	public class AttachmentContent
	{
		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("text")]
		public string Text { get; set; }

		[JsonProperty("buttons")]
		public IList<AttachmentButton> Buttons { get; set; }
	}
}
