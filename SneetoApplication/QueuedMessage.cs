using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Events;

namespace SneetoApplication
{
    public class QueuedMessage
    {
        public OnMessageReceivedArgs Event;
        public OnChatCommandReceivedArgs CommandEvent;
        public long TimeSent;
        public int Delay;
        public string Sentence;
    }
}
