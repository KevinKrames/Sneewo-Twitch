using SneetoApplication.Data_Structures;
using SneetoApplication.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Interfaces;
using TwitchLib.Client.Models;

namespace SneetoApplication
{
    public class TwitchChatClient
    {
        private static TwitchChatClient twitchChatClient;
        private TwitchCredentials twitchCredentials;
        private ITwitchClient twitchClient;

        public static TwitchChatClient Instance
        {
            get
            {
                if (twitchChatClient == null)
                {
                    twitchChatClient = new TwitchChatClient();
                }
                return twitchChatClient;
            }
            set
            {
                twitchChatClient = value;
            }
        }

        public bool IsConnected {
            get {
                if (twitchClient == null) return false;
                return twitchClient.IsConnected;
            }
        }

        public TwitchChatClient(ITwitchClient client)
        {
            twitchCredentials = TwitchCredentials.Instance;
            ConnectionCredentials credentials = new ConnectionCredentials(twitchCredentials.twitchUsername, twitchCredentials.twitchOAuth);
            twitchClient = client;
            twitchClient.Initialize(credentials);

            twitchClient.OnJoinedChannel += onJoinedChannel;
            twitchClient.OnMessageReceived += onMessageReceived;
            twitchClient.OnChatCommandReceived += OnChatCommandReceived;
        }

        public void sendMessage(string channel, string message)
        {
            if (twitchClient.IsConnected)
                twitchClient.SendMessage(twitchClient.JoinedChannels.Where(e => e.Channel.ToLower().Trim() == channel.ToLower().Trim()).FirstOrDefault().Channel, message);
        }

        private void onJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            UIManager.Instance.printMessage($"Connected to channel: {e.Channel}");
        }

        private void OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            CommandManager.Instance.channelEventsToProcess.Enqueue(e);
            UIManager.Instance.printMessage(e);
        }

        private void onMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.Message.Length > 0 && e.ChatMessage.Message[0] == Brain.configuration[Brain.COMMANDCHAR][0]) return;
            UIManager.Instance.printMessage(e);
            Brain.Instance.messagesToProcess.Enqueue(e);
        }

        public void JoinChannel(string channel)
        {
            twitchClient.JoinChannel(channel);
        }

        public void LeaveChannel(string channel)
        {
            twitchClient.LeaveChannel(channel);
        }

        public TwitchChatClient() : this(new TwitchClient()) {}

        public void Connect()
        {
            twitchClient.Connect();
            Thread.Sleep(3000);
        }
    }
}
