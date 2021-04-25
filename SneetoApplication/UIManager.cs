using SneetoApplication.Data_Structures;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Events;

namespace SneetoApplication
{
    public class UIManager
    {
        private static UIManager uiManager;
        private ConcurrentQueue<string> messagesToAppend;

        private Form1 form;
        public static UIManager Instance
        {
            get
            {
                if (uiManager == null)
                {
                    uiManager = new UIManager();
                }
                return uiManager;
            }
            set
            {
                uiManager = value;
            }
        }
        public UIManager()
        {
            messagesToAppend = new ConcurrentQueue<string>();
        }

        public void Update()
        {
            while (messagesToAppend.TryDequeue(out string value))
            {
                form.consoleTextBox.AppendText(value + "\n");
                form.consoleTextBox.SelectionStart = form.consoleTextBox.Text.Length;
                form.consoleTextBox.ScrollToCaret();
            }
        }

        public void PrintMemory()
        {
            form.richTextMemory.Clear();
            form.richTextMemory.AppendText("----------------------" + "\n");
            var channelMemoryManager = ChannelMemoryManager.Instance;
            foreach (var channelMemoryName in channelMemoryManager.Channels.Keys)
            {
                form.richTextMemory.AppendText("---" + channelMemoryName + "---" + "\n");
                var wordMemory = channelMemoryManager.Channels[channelMemoryName].WordMemory;
                foreach (var stem in wordMemory.Keys)
                {
                    form.richTextMemory.AppendText(stem.stemText + " - " + wordMemory[stem] + "\n");
                }
            }
            form.richTextMemory.AppendText("----------------------" + "\n");
        }

        public void setForm(Form1 form)
        {
            this.form = form;
        }

        public void printMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            messagesToAppend.Enqueue(message);
        }

        public void printMessage(OnMessageReceivedArgs e)
        {
            messagesToAppend.Enqueue($"#{e.ChatMessage.Channel} {e.ChatMessage.Username}: {e.ChatMessage.Message}");
        }

        public void printMessage(OnChatCommandReceivedArgs e)
        {
            messagesToAppend.Enqueue($"#{e.Command.ChatMessage.Channel} {e.Command.ChatMessage.Username}: {e.Command.ChatMessage.Message}");
        }

        internal void SendMessage(string channel, string sentence)
        {
            messagesToAppend.Enqueue($"#{channel} {TwitchCredentials.Instance.twitchUsername}: {sentence}");
        }
    }
}
