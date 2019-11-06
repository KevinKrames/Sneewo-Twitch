using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Events;

namespace SneetoApplication.Commands
{
    public class LeaveCommand : Command
    {
        public LeaveCommand()
        {
            name = "leave";
        }

        public override void Execute(OnChatCommandReceivedArgs e)
        {
            var lowerUsername = e.Command.ChatMessage.Username.ToLower();
            if (!ChannelManager.Instance.Channels.ContainsKey(lowerUsername)) return;

            ChannelManager.Instance.RemoveChannel(lowerUsername, true);
        }
    }
}
