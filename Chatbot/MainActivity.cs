using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Chatbot.Services;
using System.Threading.Tasks;
using System.Linq;
using System;
using Android.Support.V7.Widget;

namespace Chatbot
{
	[Activity(Label = "Chatbot", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{
		public EditText UserMessage { get; set; }
		public ImageButton SendButton { get; set; }
		public RecyclerView MessagesRecycler { get; set; }
		public List<Microsoft.Bot.Connector.DirectLine.Activity> MessagesList { get; set; }
		public BotConnector BotConnector { get; set; }

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.Main);

			InitBotConnector(); 
			InitViews();
			InitEvents();
		}

		private void InitViews()
		{
			UserMessage = FindViewById<EditText>(Resource.Id.main_inputmessage_edittext);
			SendButton = FindViewById<ImageButton>(Resource.Id.main_send_floatingactionbutton);
			MessagesRecycler = FindViewById<RecyclerView>(Resource.Id.main_message_listview);
		}

		private async void InitBotConnector()
		{
			var id = Android.Provider.Settings.Secure.AndroidId;
			BotConnector = new BotConnector(id);
			await BotConnector.StartBotConversation();
		}

		private void InitEvents()
		{
			SendButton.Click += (sender, e) =>
			{
				BotConnector.SendMessage(UserMessage.Text).ContinueWith((arg) => 
				{
					BotConnector.GetMessages().ContinueWith(async (result) => 
					{
						var messages = await result;
					});
				});
			};
		}
	}
}

