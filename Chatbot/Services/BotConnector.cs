using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.DirectLine;

namespace Chatbot.Services
{
	/// <summary>
	/// Bot connector
	/// Follow http://kristianbrimble.com/direct-line-v3-and-the-new-direct-line-client/
	/// </summary>
	public class BotConnector
	{
		private readonly DirectLineClient _directLineClient;
		private Conversation _conversation;
		private string _watermark;

		private string directLineSecret = "-oBb0OJjaaM.cwA.FzA.yQnOKaTnrr488oYLFAlYVDguZfBC3a93r69UXGhWAJo";
		private string botId = "DontCareBot";
		private string _fromUser = "ClientUser";

		public BotConnector(string fromUser)
		{
			_fromUser = fromUser;
			_directLineClient = new DirectLineClient(directLineSecret);
		}

		public async Task StartBotConversation()
		{
			_conversation = await _directLineClient.Conversations
												   .StartConversationAsync()
												   .ConfigureAwait(false);
		}

		public async Task SendMessage(string message)
		{
			var fromProperty = new ChannelAccount(_fromUser);
			var activity = new Activity(text: message, fromProperty: fromProperty, type: "message");
			await _directLineClient.Conversations
								   .PostActivityAsync(_conversation.ConversationId, activity)
								   .ConfigureAwait(false);
		}

		public async Task<IList<Activity>> GetMessages()
		{
			var response = await _directLineClient.Conversations
												  .GetActivitiesAsync(_conversation.ConversationId, _watermark)
												  .ConfigureAwait(false);
			_watermark = response.Watermark;
			return response.Activities;
		}
	}
}
