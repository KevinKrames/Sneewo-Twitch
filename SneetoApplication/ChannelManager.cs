using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            while (channelEventsToProcess.TryDequeue(out ChannelEvent value))
            {
                switch (value.status)
                {
                    case JOINED:
                        UIManager.Instance.printMessage($"Connected to channel: {value.name}");
                        Channels.Add(value.name, new Channel(value.name));
                        break;
                    case LEFT:
                        UIManager.Instance.printMessage($"Left channel: {value.name}");
                        Channels.Remove(value.name);
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

        public void AddChannel(OnJoinedChannelArgs e)
        {
            channelEventsToProcess.Enqueue(
                new ChannelEvent {
                    name = e.Channel,
                    status = JOINED
                }
            );
        }

        public void RemoveChannel(OnLeftChannelArgs e)
        {
            channelEventsToProcess.Enqueue(
                new ChannelEvent
                {
                    name = e.Channel,
                    status = LEFT
                }
            );
        }
    }

    public class ChannelEvent
    {
        public string name;
        public string status;
    }
}
