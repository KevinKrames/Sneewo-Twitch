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
            }
        }

        public void setForm(Form1 form)
        {
            this.form = form;
        }

        public void printMessage(string message)
        {
            messagesToAppend.Enqueue(message);
        }

        public void printMessage(OnMessageReceivedArgs e)
        {
            messagesToAppend.Enqueue($"#{e.ChatMessage.Channel} {e.ChatMessage.Username}: {e.ChatMessage.Message}");
        }

        internal void SendMessage(string channel, string sentence)
        {
            messagesToAppend.Enqueue($"#{channel} {TwitchCredentials.Instance.twitchUsername}: {sentence}");
        }
    }
}
