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
                if (value.ChatMessage.Message.Length > 0 && value.ChatMessage.Message.Substring(0, 1) == configuration[COMMANDCHAR])
                {
                    ProcessCommand(value);
                } else
                {
                    ProcessMessage(value);
                }
            }

            List<QueuedMessage> messagesToRemove = new List<QueuedMessage>();

            foreach(var message in queuedMessages)
            {
                long elapsedTicks = DateTime.Now.Ticks - message.TimeSent;
                TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
                if (elapsedSpan.TotalSeconds > message.Delay)
                {
                    messagesToRemove.Add(message);
                    TwitchChatClient.Instance.sendMessage(message.Event.ChatMessage.Channel, message.Sentence);
                    UIManager.Instance.SendMessage(message.Event.ChatMessage.Channel, message.Sentence);
                }
            }

            foreach(var message in messagesToRemove)
            {
                queuedMessages.Remove(message);
            }
        }

        private void ProcessMessage(OnMessageReceivedArgs value)
        {
            if (ignores.Contains(value.ChatMessage.Username.Trim().ToLower())) return;

            if (IsValidSentence(new TokenList(value.ChatMessage.Message)))
            {
                ChannelMemoryManager.Instance.UpdateMemoryWithMessage(value.ChatMessage.Channel, value.ChatMessage.Message);
            }

            Utilities.Utilities.AppendMessageToLog(new MessageLog {
                channel = value.ChatMessage.Channel.ToLower(),
                time = DateTime.Now.Ticks.ToString(),
                user = value.ChatMessage.Username.ToLower(),
                message = value.ChatMessage.Message
            });

            ChannelMemoryManager.Instance.AddSentence(value.ChatMessage.Channel.ToLower(), value.ChatMessage.Message);

            var channel = ChannelManager.Instance.Channels[value.ChatMessage.Channel.ToLower()];
            if (!channel.CanSpeak() || Utilities.Utilities.RandomOneToNumber(4) > 1) return;

            var sentence = GenerateSentence(value);
            if (sentence == null || sentence.Trim().ToLower() == "") return;

            ChannelMemoryManager.Instance.AddSentence(value.ChatMessage.Channel.ToLower(), sentence);

            queuedMessages.Add(new QueuedMessage
            {
                Event = value,
                Sentence = sentence,
                TimeSent = DateTime.Now.Ticks,
                Delay = 0
            });

            channel.SetSpeakTime();
        }

        private string GenerateSentence(OnMessageReceivedArgs value)
        {
            var wordList = value.ChatMessage.Message;

            var startTime = GetTimeMilliseconds();

            var runner = GPT2Runner.Instance;
            var inputSentences = String.Join("\n", ChannelMemoryManager.Instance.Channels[value.ChatMessage.Channel.ToLower()].GetMemorySentences().Select(it => it.Text));
            var messages = runner.Generate_Sentence(inputSentences);

            var message = getRandomSentence(messages);

            UIManager.Instance.printMessage("Created message in " + (GetTimeMilliseconds() - startTime)  + "ms.");

            return message;
        }

        private string getRandomSentence(string messages)
        {
            if (messages == null) return null;
            var listMessages = messages.Split('\r', '\n').ToList();
            listMessages = listMessages
                .Where(x => !String.IsNullOrWhiteSpace(x))
                .ToList();
            var messagesToRemove = new List<string>();
            var regex = "(={40}.SAMPLE.\\d.={40}|={80})";

            listMessages.ForEach(it => {
                if (Regex.Match(it, regex).Success) {
                    messagesToRemove.Add(it);
                    if (listMessages.IndexOf(it) != 0) messagesToRemove.Add(listMessages[listMessages.IndexOf(it) - 1]);
                }
                if (it.Length <= 2) messagesToRemove.Add(it);
            });

            messagesToRemove.ForEach(it =>
            {
                listMessages.Remove(it);
            });

            return listMessages.Count <= 0 ? null : listMessages[Utilities.Utilities.RandomZeroToNumberMinusOne(listMessages.Count)];
        }

        private void ProcessCommand(OnMessageReceivedArgs value)
        {
        }

        public int GetTimeMilliseconds() => (int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

        public bool IsValidSentence(TokenList tokenList)
        {
            foreach (var badWord in Brain.badWords)
            {
                if (badWord.Length == 0 || badWord[0] == '#') continue;

                if (badWord[0] == '*')
                {
                    var temp = tokenList.DoesContainAnyFormOfString(badWord.Substring(1));
                    if (temp) return false;
                }
                else if (tokenList.DoesContainWord(badWord))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
