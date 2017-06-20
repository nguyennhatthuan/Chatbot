using System;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Microsoft.Bot.Connector.DirectLine;

namespace Chatbot.Views.Chat
{
	public class ChatAdapter : RecyclerView.Adapter
	{
		private List<Activity> Messages { get; set; }
		private const int UserItemView = 1000;
		private const int BotItemView = 2000;

		public ChatAdapter(List<Activity> messages)
		{
			Messages = messages;
		}

		public override int ItemCount
		{
			get
			{
				return Messages.Count;
			}
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			var vh = holder as MessageChatbotViewHolder;
			vh.Caption.Text = Messages[position].Text;
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			if (viewType == BotItemView)
			{
				var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ReceiveMessage, parent, false);
				return new MessageChatbotViewHolder(itemView);
			}
			else
			{
				var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.SendMessage, parent, false);
				return new MessageChatbotViewHolder(itemView);
			}
		}

		public override int GetItemViewType(int position)
		{
			if (Messages[position].From.Id == "DontCareBot")
			{
				return BotItemView;
			}
			return UserItemView;
		}
	}

	public class MessageChatbotViewHolder : RecyclerView.ViewHolder
	{
		public TextView Caption { get; private set; }

		public MessageChatbotViewHolder(View itemView) : base(itemView)
		{
			Caption = itemView.FindViewById<TextView>(Resource.Id.textmessage_textview);
		}
	}
}
