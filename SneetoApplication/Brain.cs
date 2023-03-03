using Newtonsoft.Json;
using SneetoApplication.Data_Structures;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchLib.Client.Events;

namespace SneetoApplication
{
    public class Brain
    {
        public static Dictionary<string, string> configuration;
        public static List<string> badWords;
        public static List<string> smallWords;
        public static List<string> ignores;
        public static readonly string USE_DATABASE = "useDatabase";
        public static readonly string config = "configuration.json";
        public static readonly string badWordsFile = "badWords.txt";
        public static readonly string smallWordsFile = "smallWords.txt";
        public static readonly string COMMANDCHAR = "commandChar";
        public static readonly string GPT2ENDTOKEN = "<|endoftext|>";

        public static Form1 form;
        public ConcurrentQueue<OnMessageReceivedArgs> messagesToProcess;
        public List<QueuedMessage> queuedMessages;
        public Brain(Form1 form1)
        {
            form = form1;
            messagesToProcess = new ConcurrentQueue<OnMessageReceivedArgs>();
            queuedMessages = new List<QueuedMessage>();
            configuration = (Dictionary<string, string>)Utilities.Utilities.loadDictionaryFromJsonFile<string, string>(config);
            badWords = Utilities.Utilities.loadListFromTextFile(badWordsFile);
            smallWords = Utilities.Utilities.loadListFromTextFile(smallWordsFile);
            LoadIgnores();

            Instance = this;
        }

        private void LoadIgnores()
        {
            ignores = new List<string>();
            var ignoreJson = (Dictionary<string, object>)Utilities.Utilities.loadDictionaryFromJsonFile<string, object>("ignores.json");

            try
            {
                var names = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(ignoreJson["usersList"].ToString());

                foreach (var name in names)
                {
                    ignores.Add(name["name"].Trim().ToLower());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public static Brain Instance { get; set; }

        public void Update()
        {
            while (messagesToProcess.TryDequeue(out OnMessageReceivedArgs value))
            {
                ProcessMessage(value);
            }

            List<QueuedMessage> messagesToRemove = new List<QueuedMessage>();

            foreach(var message in queuedMessages)
            {
                long elapsedTicks = DateTime.Now.Ticks - message.TimeSent;
                TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
                if (elapsedSpan.TotalSeconds > message.Delay)
                {
                    messagesToRemove.Add(message);
                    var channel = message.Event != null ? message.Event.ChatMessage.Channel : message.CommandEvent.Command.ChatMessage.Channel;
                    TwitchChatClient.Instance.sendMessage(channel, message.Sentence);
                    UIManager.Instance.SendMessage(channel, message.Sentence);
                }
            }

            foreach(var message in messagesToRemove)
            {
                queuedMessages.Remove(message);
            }
        }

        private void ProcessMessage(OnMessageReceivedArgs args)
        {
            try
            {
                if (ignores.Contains(args.ChatMessage.Username.Trim().ToLower())) return;

                Utilities.Utilities.AppendMessageToLog(new MessageLog
                {
                    channel = args.ChatMessage.Channel.ToLower(),
                    time = DateTime.Now.Ticks.ToString(),
                    user = args.ChatMessage.Username.ToLower(),
                    message = args.ChatMessage.Message
                });

                var channel = ChannelManager.Instance.GetChannel(args);
                
            }
            catch (Exception e)
            {
                UIManager.Instance.printMessage($"Exception while processing message, stack trace: {e}.");
                Utilities.Utilities.WriteLineToFile(e.StackTrace, "log.txt");
            }
        }

        public long GetTimeMilliseconds() => (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

        public bool IsValidSentence(string sentence)
        {
            foreach (var badWord in Brain.badWords)
            {
                if (badWord.Length == 0 || badWord[0] == '#') continue;

                if (badWord[0] == '*')
                {
                    var temp = DoesContainAnyFormOfString(sentence, badWord.Substring(1));
                    if (temp) return false;
                }
                else if (DoesContainWord(sentence, badWord))
                {
                    return false;
                }
            }

            return true;
        }

        public bool DoesContainWord(string sentence, string word)
        {
            var wordLower = word.ToLower().Trim();
            var match = sentence.Split(' ').Where(w => w.ToLower().Trim().Equals(wordLower)).ToList();
            return match.Count > 0;
        }

        public bool DoesContainAnyFormOfString(string sentence, string word)
        {
            var wordLower = word.ToLower().Trim();
            var match = sentence.Split(' ').Where(w => w.ToLower().Trim().Contains(wordLower)).ToList();
            return match.Count > 0;
        }
    }
}
