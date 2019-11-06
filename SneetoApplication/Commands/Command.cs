using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Events;

namespace SneetoApplication.Commands
{
    public class Command
    {
        public bool ownerRequired = false;
        public string name;

        public virtual void Execute(OnChatCommandReceivedArgs e) { }

        public bool IsOwner(OnChatCommandReceivedArgs e)
        {
            return e.Command.ChatMessage.Username.Trim().ToLower() == e.Command.ChatMessage.Channel.Trim().ToLower(); 
        }
    }
}
