using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TwitchLib.Client.Events;

namespace SneetoApplication
{
    public class ChannelManager
    {
        private ConcurrentQueue<ChannelEvent> channelEventsToProcess;
        public Dictionary<string, Channel> Channels;

        private const string JOINED = "joined";
        private const string LEFT = "left";

        public ChannelManager()
        {
            Channels = new Dictionary<string, Channel>();
            channelEventsToProcess = new ConcurrentQueue<ChannelEvent>();

            var localChannels = (Dictionary<string, object>)Utilities.Utilities.loadDictionaryFromJsonFile<string, object>("channels.json");

            try
            {
                var channels = JsonConvert.DeserializeObject< List<Dictionary<string, string>>>( localChannels["channelsList"].ToString());

                foreach(var channel in channels)
                {
                    var newChannel = new Channel(channel["name"]);

                    newChannel.mute = bool.Parse(channel["mute"]);

                    newChannel.frequency = int.Parse(channel["frequency"]);

                    AddChannel(newChannel, false);
                }
            } catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public void SaveChannels()
        {
            var path = Path.GetDirectoryName(Application.ExecutablePath) + "\\files\\" + "channels.json";
            var keyValue = new Dictionary<string, Array>();

            keyValue["channelsList"] = Channels.Values.ToArray();
            string json = JsonConvert.SerializeObject(keyValue);
            File.WriteAllText(path, json);
        }

        private static ChannelManager channelManager;

        public static ChannelManager Instance
        {
            get
            {
                if (channelManager == null)
                {
                    channelManager = new ChannelManager();
                }
                return channelManager;
            }
            set
            {
                channelManager = value;
            }
        }

        public void Update()
        {
            if (!TwitchChatClient.Instance.IsConnected) return;

            while (channelEventsToProcess.TryDequeue(out ChannelEvent value))
            {
                switch (value.Status)
                {
                    case JOINED:
                        TwitchChatClient.Instance.JoinChannel(value.Name);
                        UIManager.Instance.printMessage($"Connected to channel: {value.Name}");
                        Channels.Add(value.Name, value.Channel);
                        if (value.ShouldSave) SaveChannels();
                        break;
                    case LEFT:
                        TwitchChatClient.Instance.LeaveChannel(value.Name);
                        UIManager.Instance.printMessage($"Left channel: {value.Name}");
                        Channels.Remove(value.Name);
                        if (value.ShouldSave) SaveChannels();
                        break;
                    default:
                        break;
                }
            }

            foreach(var channel in Channels.Values)
            {
                channel.Update();
            }
        }

        public void AddChannel(string channel, bool shouldSave)
        {
            AddChannel(new Channel(channel), shouldSave);
        }

        public void RemoveChannel(string channel, bool shouldSave)
        {
            RemoveChannel(Channels[channel], shouldSave);
        }

        public void AddChannel(Channel channel, bool shouldSave)
        {
            channelEventsToProcess.Enqueue(
                new ChannelEvent {
                    Name = channel.name,
                    Channel = channel,
                    Status = JOINED,
                    ShouldSave = shouldSave
                }
            );
        }

        public void RemoveChannel(Channel channel, bool shouldSave)
        {
            channelEventsToProcess.Enqueue(
                new ChannelEvent
                {
                    Name = channel.name,
                    Status = LEFT,
                    ShouldSave = shouldSave
                }
            );
        }
    }

    public class ChannelEvent
    {
        public string Name;
        public Channel Channel;
        public string Status;
        public bool ShouldSave;
    }
}
