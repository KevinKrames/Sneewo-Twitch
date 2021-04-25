using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Events;

namespace SneetoApplication.GPT2
{
    public class GPT2Message
    {
        public string inputText;
        public string outputText;
        public OnMessageReceivedArgs args;
        public long startTime;
    }
}
