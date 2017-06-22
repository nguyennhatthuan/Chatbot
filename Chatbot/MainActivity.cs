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
using Android.Views.InputMethods;
using Android.Content;

namespace Chatbot
{
    [Activity(Label = "Chatbot", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        #region Views
        private EditText UserMessage { get; set; }
        private Button SendButton { get; set; }
        private RecyclerView MessagesRecycler { get; set; }
        private LinearLayout InputArea { get; set; }
        private RelativeLayout TextInputLayout { get; set; }
        private HorizontalScrollView ButtonsInputLayout { get; set; }
        #endregion

        private List<Microsoft.Bot.Connector.DirectLine.Activity> MessagesList { get; set; } = new List<Microsoft.Bot.Connector.DirectLine.Activity>();
        private ChatAdapter Adapter { get; set; }
        private BotConnector BotConnector { get; set; }

        private bool _datePicker = false;

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

            Adapter = new ChatAdapter(MessagesList);
            var layoutManager = new LinearLayoutManager(this);
            layoutManager.ReverseLayout = true;
            layoutManager.StackFromEnd = true;
            MessagesRecycler.SetLayoutManager(layoutManager);
            MessagesRecycler.SetAdapter(Adapter);

            SendButton.Enabled = false;
            SetInputLayout(true);
        }

        private async void InitBotConnector()
        {
            var id = Android.Provider.Settings.Secure.AndroidId;
            BotConnector = new BotConnector(id);
            BotConnector.StartBotConversation().GetAwaiter().OnCompleted(() =>
            {
                SendButton.Enabled = true;
            });
        }

        private void InitEvents()
        {
            SendButton.Click += SendButton_Click;
        }

        private async void SendButton_Click(object sender, EventArgs e)
        {
            if (_datePicker == true)
            {
                UserMessage.FocusableInTouchMode = true;
                UserMessage.Click -= UserMessage_Click;
            }

            HideKeyboard();

            var message = UserMessage.Text;
            UserMessage.Text = string.Empty;
            var activity = new Microsoft.Bot.Connector.DirectLine.Activity("message", text: message, fromProperty: new Microsoft.Bot.Connector.DirectLine.ChannelAccount { Id = Android.Provider.Settings.Secure.AndroidId });
            AddMessageToList(activity);
            await SendMessage(message);
        }

        private void HideKeyboard()
        {
            var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
            inputManager.HideSoftInputFromWindow(CurrentFocus.WindowToken, HideSoftInputFlags.NotAlways);
        }

        private async Task SendMessage(string message)
        {
            TextInputLayout.Visibility = ViewStates.Gone;
            ButtonsInputLayout.Visibility = ViewStates.Gone;

            await BotConnector.SendMessage(message);
            var result = await BotConnector.GetMessages();
            UpdateListMessages(result.ToList());
        }

        private void UpdateListMessages(List<Microsoft.Bot.Connector.DirectLine.Activity> messages)
        {
            foreach (var message in messages)
            {
                if (MessageChecker.CheckTypeOfMessage(message) == AttachmentType.None)
                {
                    AddMessageToList(message);

                    if (MessageChecker.CheckInputType(message.Text) == InputType.Birthday)
                    {
                        AddDatePicker();
                    }
                    else
                    {
                        _datePicker = false;
                    }

                    SetInputLayout(true);
                }
                else
                {
                    var attachmentContent = JsonConvert.DeserializeObject<AttachmentContent>(message.Attachments[0].Content.ToString());
                    message.Text = attachmentContent.Text;

                    SetInputLayout(false);

                    AddMessageToList(message);
                    AddButtons(attachmentContent.Buttons.ToList());
                }
            }
        }

        private void AddMessageToList(Microsoft.Bot.Connector.DirectLine.Activity message)
        {
            MessagesList.Insert(0, message);
            Adapter.NotifyItemInserted(0);
            MessagesRecycler.ScrollToPosition(0);
        }

        private void AddButtons(List<AttachmentButton> attachmentButtons)
        {
            var layoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            layoutParams.Weight = 1;
            layoutParams.SetMargins(4, 2, 4, 2);

            var width = 0;
            foreach (var attachmentButton in attachmentButtons)
            {
                var button = new Button(this)
                {
                    Text = attachmentButton.Title,
                    LayoutParameters = layoutParams,
                };
                button.SetPadding(4, 2, 4, 2);

                var drawable = Resources.GetDrawable(Resource.Drawable.button_rounded);
                button.Background = drawable;
                button.Click += async (sender, e) =>
                {
                    InputArea.RemoveAllViews();

                    var activity = new Microsoft.Bot.Connector.DirectLine.Activity(type: "message",
                                                                                   text: button.Text,
                                                                                   fromProperty: new Microsoft.Bot.Connector.DirectLine.ChannelAccount { Id = Android.Provider.Settings.Secure.AndroidId });
                    AddMessageToList(activity);
                    await SendMessage(button.Text);
                };
                InputArea.AddView(button);
                width += button.Width;
            }
        }

        private void AddDatePicker()
        {
            UserMessage.FocusableInTouchMode = false;
            UserMessage.Click += UserMessage_Click;
            _datePicker = true;
        }

        private void UserMessage_Click(object sender, EventArgs e)
        {
            var currently = DateTime.Now;
            var dialog = new DatePickerDialog(this, (dateSender, args) =>
            {
                UserMessage.Text = $"{args.DayOfMonth}/{args.Month + 1}/{args.Year}";
            }, currently.Year, currently.Month - 1, currently.Day);
            dialog.Show();
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

