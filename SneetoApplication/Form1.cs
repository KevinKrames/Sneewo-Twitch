using SneetoApplication.Data_Structures;
using SneetoApplication.PythonInstances;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SneetoApplication
{
    public partial class Form1 : Form
    {
        TwitchChatClient twitchChatClient;
        private Brain brain;
        private bool isQuitting;

        public Form1()
        {
            InitializeComponent();
            brain = new Brain(this);
            UIManager.Instance.setForm(this);
            ChatGPTPython.StartPythonThread();
            TTSPython.StartPythonThread();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            twitchChatClient = TwitchChatClient.Instance;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            twitchChatClient.Connect();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //string data = "";
            //try
            //{
            //    StreamReader sr = new StreamReader(Path.GetDirectoryName(Application.ExecutablePath) + "\\files\\log.txt");
            //    while (!sr.EndOfStream)
            //    {
            //        data = sr.ReadLine();

            //        var dictionary = new Dictionary<string, string>();

            //        var startIndex = 0;
            //        var index = data.IndexOf(' ', 0);
            //        var channel = data.Substring(startIndex, index - startIndex);

            //        dictionary.Add("channel", channel);

            //        startIndex = index + 1;
            //        index = data.IndexOf(' ', startIndex);
            //        var platform = data.Substring(startIndex, index - startIndex);

            //        dictionary.Add("platform", platform);

            //        startIndex = index + 1;
            //        index = data.IndexOf(' ', startIndex);
            //        var time = data.Substring(startIndex, index - startIndex);

            //        dictionary.Add("time", time);

            //        startIndex = index + 1;
            //        index = data.IndexOf(':', startIndex);
            //        var user = data.Substring(startIndex, index - startIndex);

            //        dictionary.Add("user", user);

            //        var message = data.Substring(index + 1);

            //        dictionary.Add("message", message);

            //        Utilities.Utilities.WriteLineToFile(Utilities.Utilities.jsonSerialize(dictionary) + ",", TokenMemoryManager.DATA_FILE_NAME);
            //        System.Console.WriteLine(@"{channel}, {platform}, {time}, {user}, {message}");
            //    }
            //    sr.Close();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Error reading file");
            //    Console.WriteLine(ex.StackTrace);
            //}
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void consoleTextBox_TextChanged(object sender, EventArgs e)
        {
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //consoleTextBox.AppendText(brain.GenerateRandomSentence() + "\n");
        }

        private void stemButton_Click(object sender, EventArgs e)
        {
            //var stem = stemBox.Text;
            //var tokens = StemManager.GetTokensForUnstemmedWord(stem);

            //foreach(var token in tokens)
            //{
            //    consoleTextBox.AppendText(token.WordText + "\n");
            //}
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (isQuitting && ChatGPTPython.HasExited() && TTSPython.HasExited())
            {
                System.Environment.Exit(0);
            }
            try
            {
                UIManager.Instance.Update();
                ChannelManager.Instance.Update();
                CommandManager.Instance.Update();
                Brain.Instance.Update();
                RequestManager.Instance.Update();
            } catch (Exception exc)
            {
                Utilities.Utilities.WriteLineToFile($"{exc.StackTrace}, {exc.Message}, {exc.Source}", "log.txt");
            }
        }

        private void richTextMemory_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            isQuitting = true;
            if (ChatGPTPython.HasExited() == false)
                ChatGPTPython.process.Kill();
            Thread.Sleep(1000);
            if (TTSPython.HasExited() == false)
                TTSPython.process.Kill();
            Thread.Sleep(1000);
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            twitchChatClient.Disconnect();
        }
    }
}
