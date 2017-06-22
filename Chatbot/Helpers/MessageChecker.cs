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

        public static InputType CheckInputType(string message)
        {
            message = message.ToLower();
            if(message.Contains("birthday?"))
            {
                return InputType.Birthday;
            }
            return InputType.Normal;
        }
	}
}
