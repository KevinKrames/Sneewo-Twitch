using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneetoApplication
{
    public class ChannelMemoryManager
    {
        public Dictionary<string, ChannelMemory> Channels;
        private static ChannelMemoryManager channelMemoryManager;
        private int updateTime = 0;

        public static ChannelMemoryManager Instance
        {
            get
            {
                if (channelMemoryManager == null)
                {
                    channelMemoryManager = new ChannelMemoryManager();
                }
                return channelMemoryManager;
            }
            set
            {
                channelMemoryManager = value;
            }
        }

        public ChannelMemoryManager()
        {
            Channels = new Dictionary<string, ChannelMemory>();
        }

        public void Update()
        {
            updateTime++;
            if (updateTime > 30)
            {
                foreach (var memory in Channels.Values)
                {
                    memory.Update();
                }
                updateTime = 0;
                UIManager.Instance.PrintMemory();
            }
        }

        public void UpdateMemoryWithMessage(string channel, string message)
        {
            if (!Channels.ContainsKey(channel)) AddChannel(channel);
            Channels[channel].UpdateMessage(message);
            UIManager.Instance.PrintMemory();
        }

        private void AddChannel(string channel)
        {
            Channels[channel] = new ChannelMemory();
        }
    }
}
