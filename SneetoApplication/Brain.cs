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
    public class Brain
    {
        public static Dictionary<string, string> configuration;
        public static List<string> badWords;
        public static List<string> smallWords;
        public static readonly string USE_DATABASE = "useDatabase";
        public static readonly string config = "configuration.json";
        public static readonly string badWordsFile = "badWords.txt";
        public static readonly string smallWordsFile = "smallWords.txt";
        public static readonly string COMMANDCHAR = "commandChar";
        public static Form1 form;
        public ConcurrentQueue<OnMessageReceivedArgs> messagesToProcess;
        public List<QueuedMessage> queuedMessages;
        private TokenMemoryManager tokenMemoryManager;
        public Brain(Form1 form1)
        {
            form = form1;
            messagesToProcess = new ConcurrentQueue<OnMessageReceivedArgs>();
            queuedMessages = new List<QueuedMessage>();
            configuration = (Dictionary<string, string>)Utilities.Utilities.loadDictionaryFromJsonFile<string, string>(config);
            badWords = Utilities.Utilities.loadListFromTextFile(badWordsFile);
            smallWords = Utilities.Utilities.loadListFromTextFile(smallWordsFile);
            tokenMemoryManager = new TokenMemoryManager();
            Instance = this;
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
            if (!tokenMemoryManager.TrainSingleSentence(new TokenList(value.ChatMessage.Message))) return;

            Utilities.Utilities.AppendMessageToLog(new MessageLog {
                channel = value.ChatMessage.Channel.ToLower(),
                time = DateTime.Now.Ticks.ToString(),
                user = value.ChatMessage.Username.ToLower(),
                message = value.ChatMessage.Message
            });

            ChannelMemoryManager.Instance.UpdateMemoryWithMessage(value.ChatMessage.Channel, value.ChatMessage.Message);

            var channel = ChannelManager.Instance.Channels[value.ChatMessage.Channel.ToLower()];
            if (!channel.CanSpeak()) return;

            var sentence = GenerateRandomSentence(new TokenList(value.ChatMessage.Message));
            if (sentence == null) return;

            queuedMessages.Add(new QueuedMessage
            {
                Event = value,
                Sentence = sentence,
                TimeSent = DateTime.Now.Ticks,
                Delay = Utilities.Utilities.RandomOneToNumber(3)
            });

            channel.SetSpeakTime();
        }

        private void ProcessCommand(OnMessageReceivedArgs value)
        {
        }

        public string TimedGenerateSentence(string sourceSentence, int milisecondsToGenerate)
        {
            var result = "";

            var wordList = new TokenList(sourceSentence);

            //tokenMemoryManager.UpdateUsedWords(wordList);

            var timeStarted = GetTimeMilliseconds();

            //while (GetTimeMilliseconds() - timeStarted < milisecondsToGenerate)
            //{
            //    result = GenerateRandomSentence(wordList);
            //}

            return result;
        }

        public int GetTimeMilliseconds() => (int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

        public string GetRandomWord(TokenList tokenList)
        {
            if (!tokenList.Any()) return null;

            var majorWords = tokenList.GetMajorWords();
            if (!majorWords.Any()) return null;

            return majorWords[Utilities.Utilities.RandomZeroToNumberMinusOne(majorWords.Count)];
        }

        public string GenerateRandomSentence(TokenList tokenList)
        {
            var word = GetRandomWord(tokenList);
            if (word == null) { return null; }
            var tokens = StemManager.GetTokensForUnstemmedWord(word);
            if (tokens == null || tokens.Count == 0) { return null; }
            var token = tokens[Utilities.Utilities.RandomZeroToNumberMinusOne(tokens.Count)];

            if (token.reverse) token = TokenManager.GetTokenForID(token.PartnerID);
            var reverseToken = TokenManager.GetTokenForID(token.PartnerID);

            return $"{GetNextRandomTokenString(reverseToken, true)} {token.WordText} {GetNextRandomTokenString(token, false)}";
        }

        private string GetNextRandomTokenString(Token token, bool reverse)
        {
            if (token.ChildrenTokens == null) return "";

            var number = Utilities.Utilities.RandomZeroToNumberMinusOne(token.ChildrenTokens.Count);
            Token tempToken = TokenManager.GetTokenForID(token.ChildrenTokens[number]);
            
            if (reverse)
            {
                return GetNextRandomTokenString(tempToken, reverse) + " " + tempToken.WordText;
            } else
            {
                return tempToken.WordText + " " + GetNextRandomTokenString(tempToken, reverse);
            }
        }
    }
}
