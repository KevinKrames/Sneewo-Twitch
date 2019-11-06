using SneetoApplication.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Events;

namespace SneetoApplication
{
    public class CommandManager
    {
        private static CommandManager commandManager;
        public ConcurrentQueue<OnChatCommandReceivedArgs> channelEventsToProcess;
        private List<Command> commands;

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
            }
        }
    }
}
