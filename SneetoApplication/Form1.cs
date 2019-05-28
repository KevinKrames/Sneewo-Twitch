using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SneetoApplication
{
    public partial class Form1 : Form
    {
        TwitchChatClient twitchChatClient;
        Brain brain;
        public Form1()
        {
            InitializeComponent();
            brain = new Brain(this);
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
            string data = "";
            try
            {
                StreamReader sr = new StreamReader(Path.GetDirectoryName(Application.ExecutablePath) + "\\files\\log.txt");
                while (!sr.EndOfStream)
                {
                    data = sr.ReadLine();

                    var dictionary = new Dictionary<string, string>();

                    var startIndex = 0;
                    var index = data.IndexOf(' ', 0);
                    var channel = data.Substring(startIndex, index - startIndex);

                    dictionary.Add("channel", channel);

                    startIndex = index + 1;
                    index = data.IndexOf(' ', startIndex);
                    var platform = data.Substring(startIndex, index - startIndex);

                    dictionary.Add("platform", platform);

                    startIndex = index + 1;
                    index = data.IndexOf(' ', startIndex);
                    var time = data.Substring(startIndex, index - startIndex);

                    dictionary.Add("time", time);

                    startIndex = index + 1;
                    index = data.IndexOf(':', startIndex);
                    var user = data.Substring(startIndex, index - startIndex);

                    dictionary.Add("user", user);

                    var message = data.Substring(index + 1);

                    dictionary.Add("message", message);

                    Utilities.Utilities.WriteLineToFile(Utilities.Utilities.jsonSerialize(dictionary) + ",", TokenMemoryManager.DATA_FILE_NAME);
                    System.Console.WriteLine(@"{channel}, {platform}, {time}, {user}, {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading file");
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void consoleTextBox_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
