using SneetoApplication.Data_Structures;
using SneetoApplication.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public TwitchChatClient(ITwitchClient client)
        {
            twitchCredentials = TwitchCredentials.Instance;
            ConnectionCredentials credentials = new ConnectionCredentials(twitchCredentials.twitchUsername, twitchCredentials.twitchOAuth);
            twitchClient = client;
            twitchClient.Initialize(credentials, "Moltov");

            twitchClient.OnJoinedChannel += onJoinedChannel;
            twitchClient.OnMessageReceived += onMessageReceived;
        }

        public void sendMessage(string channel, string message)
        {
            if (twitchClient.IsConnected)
                twitchClient.SendMessage(twitchClient.JoinedChannels.Where(e => e.Channel.ToLower().Trim() == channel.ToLower().Trim()).FirstOrDefault().Channel, message);
        }

        private void onJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            ChannelManager.Instance.AddChannel(e);
        }

        private void onMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            UIManager.Instance.printMessage(e);
            Brain.Instance.messagesToProcess.Enqueue(e);
        }

        public TwitchChatClient() : this(new TwitchClient()) {}

        public void Connect()
        {
            twitchClient.Connect();
        }
    }
}
