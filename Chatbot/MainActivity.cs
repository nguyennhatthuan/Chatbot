using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Chatbot.Services;
using System.Threading.Tasks;
using System.Linq;
using System;
using Android.Support.V7.Widget;
using Chatbot.Views.Chat;
using Chatbot.Definitions;
using Chatbot.Helpers;
using Chatbot.Definitions.Models;
using Newtonsoft.Json;
using Android.Views;
using Android.Support.V4.Content;

namespace Chatbot
{
	[Activity(Label = "Chatbot", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{
		public EditText UserMessage { get; set; }
		public Button SendButton { get; set; }
		public RecyclerView MessagesRecycler { get; set; }
		public LinearLayout InputArea { get; set; }
		public RelativeLayout TextInputLayout { get; set; }
		public HorizontalScrollView ButtonsInputLayout { get; set; }
		public List<Microsoft.Bot.Connector.DirectLine.Activity> MessagesList { get; set; } = new List<Microsoft.Bot.Connector.DirectLine.Activity>();
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
			SendButton = FindViewById<Button>(Resource.Id.main_send_floatingactionbutton);
			MessagesRecycler = FindViewById<RecyclerView>(Resource.Id.main_message_listview);
			TextInputLayout = FindViewById<RelativeLayout>(Resource.Id.main_textinput_layout);
			ButtonsInputLayout = FindViewById<HorizontalScrollView>(Resource.Id.main_buttoninput_layout);
			InputArea = FindViewById<LinearLayout>(Resource.Id.main_inputmessage_layout);

			var adapter = new ChatAdapter(MessagesList);
			var layoutManager = new LinearLayoutManager(this);
			layoutManager.ReverseLayout = true;
			layoutManager.StackFromEnd = true;
			MessagesRecycler.SetLayoutManager(layoutManager);
			MessagesRecycler.SetAdapter(adapter);

			SetInputLayout(true);
		}

		private async void InitBotConnector()
		{
			var id = Android.Provider.Settings.Secure.AndroidId;
			BotConnector = new BotConnector(id);
			BotConnector.StartBotConversation();
		}

		private void InitEvents()
		{
			SendButton.Click += SendButton_Click;
		}

		private async void SendButton_Click(object sender, EventArgs e)
		{
			var message = UserMessage.Text;
			UserMessage.Text = string.Empty;

			var activity = new Microsoft.Bot.Connector.DirectLine.Activity(type: "message", text: message, fromProperty: new Microsoft.Bot.Connector.DirectLine.ChannelAccount { Id = Android.Provider.Settings.Secure.AndroidId });
			AddMessageToList(activity);
			await SendMessage(message);
		}

		private async Task SendMessage(string message)
		{
			await BotConnector.SendMessage(message);
			var result = await BotConnector.GetMessages();
			UpdateListMessages(result.ToList());
		}

		private void UpdateListMessages(List<Microsoft.Bot.Connector.DirectLine.Activity> messages)
		{
			RunOnUiThread(() =>
			{
				foreach (var message in messages)
				{
					if (MessageChecker.CheckTypeOfMessage(message) == AttachmentType.None)
						AddMessageToList(message);
					else
					{
						var attachmentContent = JsonConvert.DeserializeObject<AttachmentContent>(message.Attachments[0].Content.ToString());
						message.Text = attachmentContent.Text;

						SetInputLayout(false);

						AddMessageToList(message);
						AddButtons(attachmentContent.Buttons.ToList());
					}
				}
			});      
		}

		private void AddMessageToList(Microsoft.Bot.Connector.DirectLine.Activity message)
		{
			MessagesList.Insert(0, message);
			MessagesRecycler.GetAdapter().NotifyItemInserted(0);
			MessagesRecycler.ScrollToPosition(0);
		}

		private void AddButtons(List<AttachmentButton> attachmentButtons)
		{
			var layoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
			layoutParams.Weight = 1;
			layoutParams.SetMargins(4, 2, 4, 2);
			foreach (var attachmentButton in attachmentButtons)
			{
				var button = new Button(this)
				{
					Text = attachmentButton.Title,
					LayoutParameters = layoutParams,
				};

				var drawable = Resources.GetDrawable(Resource.Drawable.button_rounded);
				button.Background = drawable;
				button.Click += async (sender, e) =>
				{
					InputArea.RemoveAllViews();
					SetInputLayout(true);
					var activity = new Microsoft.Bot.Connector.DirectLine.Activity(type: "message", text: button.Text, fromProperty: new Microsoft.Bot.Connector.DirectLine.ChannelAccount { Id = Android.Provider.Settings.Secure.AndroidId });
					AddMessageToList(activity);
					await SendMessage(button.Text);
				};
				InputArea.AddView(button);
			}
		}

		private void SetInputLayout(bool condition)
		{
			if (condition)
			{
				TextInputLayout.Visibility = ViewStates.Visible;
				ButtonsInputLayout.Visibility = ViewStates.Gone;
			}
			else
			{
				TextInputLayout.Visibility = ViewStates.Gone;
				ButtonsInputLayout.Visibility = ViewStates.Visible;
			}
		}
	}
}

