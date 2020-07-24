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
        public static readonly string TIMERREGEX = "(!timer \\d*$)";
        public static readonly string CHANCEREGEX = "(!chance \\d*$)";
        public static readonly string MESSAGEHELP = "List of Commands: !join | !leave | !mute | !unmute | !timer 15 (can't send more than a message every 15 seconds) | !chance 15 (e.g. 15% chance to speak)";

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
                new JoinCommand(),
                new LeaveCommand()
            };
        }

        public void Update()
        {
            while (channelEventsToProcess.TryDequeue(out OnChatCommandReceivedArgs value))
            {
                var command = commands.Where(c => c.name == value.Command.CommandText.ToLower()).FirstOrDefault();
                if (command != null) command.Execute(value);

                try
                {
                    if (!value.Command.ChatMessage.IsModerator && !value.Command.ChatMessage.IsBroadcaster) return;

                    var channel = ChannelManager.Instance.GetChannel(value);

                    if (value.Command.ChatMessage.Message.Substring(1).Trim().ToLower() == ChannelManager.MUTE)
                    {
                        ChannelManager.Instance.Mute(channel, value);
                    }

                    if (value.Command.ChatMessage.Message.Substring(1).Trim().ToLower() == ChannelManager.UNMUTE)
                    {
                        ChannelManager.Instance.Unmute(channel, value);
                    }

                    if (value.Command.ChatMessage.Message.Substring(1).Trim().ToLower() == ChannelManager.HELP)
                    {
                        Brain.Instance.queuedMessages.Add(new QueuedMessage
                        {
                            CommandEvent = value,
                            Sentence = MESSAGEHELP,
                            TimeSent = DateTime.Now.Ticks,
                            Delay = 0
                        });
                    }

                    if (Regex.Match(value.Command.ChatMessage.Message.Trim().ToLower(), TIMERREGEX).Success)
                    {
                        ChannelManager.Instance.SetFrequency(channel, value);
                    }

                    if (Regex.Match(value.Command.ChatMessage.Message.Trim().ToLower(), CHANCEREGEX).Success)
                    {
                        ChannelManager.Instance.SetChance(channel, value);
                    }
                }
                catch (Exception e)
                {
                    UIManager.Instance.printMessage($"Exception while processing message, stack trace: {e}.");
                }
            }
        }
    }
}
