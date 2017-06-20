using System;
using Chatbot.Definitions;

namespace Chatbot.Helpers
{
	public static class MessageChecker
	{
		public static AttachmentType CheckTypeOfMessage(Microsoft.Bot.Connector.DirectLine.Activity message)
		{
			switch (message.AttachmentLayout)
			{
				case "list":
					return AttachmentType.List;
				default:
					return AttachmentType.None;
			}
		}
	}
}
