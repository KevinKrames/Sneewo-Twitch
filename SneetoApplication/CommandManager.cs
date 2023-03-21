using SneetoApplication.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchLib.Client.Events;

namespace SneetoApplication
{
    public class CommandManager
    {
        private static CommandManager commandManager;
        public ConcurrentQueue<OnChatCommandReceivedArgs> channelEventsToProcess;
        private List<Command> commands;
        private bool isMuted = false;

        public static readonly string TIMERREGEX = "(!timer \\d*$)";
        public static readonly string CHANCEREGEX = "(!chance \\d*$)";
        public static readonly string MESSAGEHELP = "List of Commands: !help | !request {Subject of Episode}";

        

        public static CommandManager Instance
        {
            get
            {
                if (commandManager == null)
                {
                    commandManager = new CommandManager();
                }
                return commandManager;
            }
            set
            {
                commandManager = value;
            }
        }

        public CommandManager()
        {
            channelEventsToProcess = new ConcurrentQueue<OnChatCommandReceivedArgs>();
            commands = new List<Command>
            {
                
            };
        }

        public void Update()
        {
            while (channelEventsToProcess.TryDequeue(out OnChatCommandReceivedArgs value))
            {
                //var command = commands.Where(c => c.name == value.Command.CommandText.ToLower()).FirstOrDefault();
                //if (command != null) command.Execute(value);

                try
                {

                    var channel = ChannelManager.Instance.GetChannel(value);
                    var message = value.Command.ChatMessage.Message.Substring(1).Trim().ToLower();

                    if (message == ChannelManager.HELP)
                    {
                        Brain.Instance.queuedMessages.Add(new QueuedMessage
                        {
                            CommandEvent = value,
                            Sentence = MESSAGEHELP,
                            TimeSent = DateTime.Now.Ticks,
                            Delay = 0
                        });

                    }


                    if (message.StartsWith(ChannelManager.REQUESTS))
                    {
                        TwitchChatClient.Instance.sendMessage(channel.name, $"Requests are " + (isMuted ? "closed" : "open, use !request to suggest a theme for an episode!"));
                    }
                    else if (message.StartsWith(ChannelManager.REQUEST))
                    {
                        if (isMuted)
                        {
                            TwitchChatClient.Instance.sendMessage(channel.name, $"Requests are currently turned off, please try again later.");
                        } else
                        {
                            var index = message.IndexOf(" ") + 1;
                            var split = message.Substring(index, message.Length - index);
                            if (index == 0)
                            {
                                split = "";
                            }
                            RequestManager.Instance.CreateRequestEvent(value.Command.ChatMessage.Username, split, channel);
                        }
                    }

                    if (!value.Command.ChatMessage.IsModerator && !value.Command.ChatMessage.IsBroadcaster) return;

                    if (message == ChannelManager.CLEAR)
                    {
                        RequestManager.Instance.Clearing = true;
                        TwitchChatClient.Instance.sendMessage(channel.name, $"Messages are being purged please wait.");
                    }

                    if (message == ChannelManager.MUTE)
                    {
                        isMuted = true;
                        TwitchChatClient.Instance.sendMessage(channel.name, $"Requests have been muted.");
                    }

                    if (message == ChannelManager.UNMUTE)
                    {
                        isMuted = false;
                        TwitchChatClient.Instance.sendMessage(channel.name, $"Requests have been unmuted.");
                    }

                    if (message == ChannelManager.RESET)
                    {
                        RequestManager.Instance.ResetPythonAndRequests();
                        TwitchChatClient.Instance.sendMessage(channel.name, $"Rebooting the request system.");
                    }

                    //if (Regex.Match(message, TIMERREGEX).Success)
                    //{
                    //    ChannelManager.Instance.SetFrequency(channel, value);
                    //}

                    //if (Regex.Match(message, CHANCEREGEX).Success)
                    //{
                    //    ChannelManager.Instance.SetChance(channel, value);
                    //}
                }
                catch (Exception e)
                {
                    UIManager.Instance.printMessage($"Exception while processing message, stack trace: {e}.");
                }
            }
        }
    }
}
