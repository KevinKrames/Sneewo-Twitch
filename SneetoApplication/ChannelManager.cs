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

        public const string JOINED = "joined";
        public const string LEFT = "left";
        public const string MUTE = "mute";
        public const string CLEAR = "clear";
        public const string REQUEST = "request";
        public const string REQUESTS = "requests";
        public const string RESET = "reset";
        public const string UNMUTE = "unmute";
        public const string HELP = "help";
        public const string FREQUENCY = "frequency";
        public const string CHANCE = "chance";

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

                    newChannel.chance = int.Parse(channel["chance"]);

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
                        Channels.Add(value.Name, value.Channel);
                        break;
                    case LEFT:
                        TwitchChatClient.Instance.LeaveChannel(value.Name);
                        UIManager.Instance.printMessage($"Left channel: {value.Name}");
                        Channels.Remove(value.Name);
                        break;
                    case MUTE:
                        value.Channel.mute = true;
                        UIManager.Instance.printMessage($"Muted in {value.Channel.name}.");
                        Brain.Instance.queuedMessages.Add(new QueuedMessage
                        {
                            CommandEvent = value.commandArgs,
                            Sentence = "Muted.",
                            TimeSent = DateTime.Now.Ticks,
                            Delay = 0
                        });
                        break;
                    case UNMUTE:
                        value.Channel.mute = false;
                        UIManager.Instance.printMessage($"Unmuted in {value.Channel.name}.");
                        Brain.Instance.queuedMessages.Add(new QueuedMessage
                        {
                            CommandEvent = value.commandArgs,
                            Sentence = "Unmuted.",
                            TimeSent = DateTime.Now.Ticks,
                            Delay = 0
                        });
                        break;
                    case FREQUENCY:
                        var freq = int.Parse(value.commandArgs.Command.ChatMessage.Message.Split(' ')[1]);
                        value.Channel.frequency = freq;
                        UIManager.Instance.printMessage($"Cooldown timer set to {freq}.");
                        Brain.Instance.queuedMessages.Add(new QueuedMessage
                        {
                            CommandEvent = value.commandArgs,
                            Sentence = $"Cooldown timer set to {freq}.",
                            TimeSent = DateTime.Now.Ticks,
                            Delay = 0
                        });
                        break;
                    case CHANCE:
                        var chance = int.Parse(value.commandArgs.Command.ChatMessage.Message.Split(' ')[1]);
                        value.Channel.chance = chance;
                        UIManager.Instance.printMessage($"Chance to speak set to {chance}%.");
                        Brain.Instance.queuedMessages.Add(new QueuedMessage
                        {
                            CommandEvent = value.commandArgs,
                            Sentence = $"Chance to speak set to {chance}%.",
                            TimeSent = DateTime.Now.Ticks,
                            Delay = 0
                        });
                        break;
                    default:
                        break;
                }
                if (value.ShouldSave) SaveChannels();
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

        public Channel GetChannel(OnMessageReceivedArgs value)
        {
            var channelName = value.ChatMessage.Channel.Trim().ToLower();
            if (Channels.ContainsKey(channelName)) return Channels[channelName];
            throw new Exception("Could not find channel when retrieving.");
        }

        public Channel GetChannel(OnChatCommandReceivedArgs value)
        {
            var channelName = value.Command.ChatMessage.Channel.Trim().ToLower();
            if (Channels.ContainsKey(channelName)) return Channels[channelName];
            throw new Exception("Could not find channel when retrieving.");
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

        public void Mute(Channel channel, OnChatCommandReceivedArgs args)
        {
            channelEventsToProcess.Enqueue(
                new ChannelEvent
                {
                    Name = channel.name,
                    Channel = GetChannel(args),
                    Status = MUTE,
                    ShouldSave = true,
                    commandArgs = args
                }
            );
        }

        public void Unmute(Channel channel, OnChatCommandReceivedArgs args)
        {
            channelEventsToProcess.Enqueue(
                new ChannelEvent
                {
                    Name = channel.name,
                    Channel = GetChannel(args),
                    Status = UNMUTE,
                    ShouldSave = true,
                    commandArgs = args
                }
            );
        }

        internal void SetFrequency(Channel channel, OnChatCommandReceivedArgs args)
        {
            channelEventsToProcess.Enqueue(
                new ChannelEvent
                {
                    Name = channel.name,
                    Channel = GetChannel(args),
                    Status = FREQUENCY,
                    ShouldSave = true,
                    commandArgs = args
                }
            );
        }

        internal void SetChance(Channel channel, OnChatCommandReceivedArgs args)
        {
            channelEventsToProcess.Enqueue(
                new ChannelEvent
                {
                    Name = channel.name,
                    Channel = GetChannel(args),
                    Status = CHANCE,
                    ShouldSave = true,
                    commandArgs = args
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
        public OnMessageReceivedArgs messageArgs;
        public OnChatCommandReceivedArgs commandArgs;
    }
}
