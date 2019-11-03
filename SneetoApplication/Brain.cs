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
        private TokenMemoryManager tokenMemoryManager;
        public Brain(Form1 form1)
        {
            form = form1;
            messagesToProcess = new ConcurrentQueue<OnMessageReceivedArgs>();
            configuration = Utilities.Utilities.loadDictionaryFromJsonFile(config);
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
        }

        private void ProcessMessage(OnMessageReceivedArgs value)
        {
            var sentence = GenerateRandomSentence(new TokenList(value.ChatMessage.Message));
            if (sentence == null) return;

            TwitchChatClient.Instance.sendMessage(value.ChatMessage.Channel, sentence);
            UIManager.Instance.SendMessage(value.ChatMessage.Channel, sentence);
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
            var reverseToken = TokenManager.GetTokenForID(token.PartnerID);

            return $"{GetNextRandomTokenString(reverseToken, true)} {token.WordText} {GetNextRandomTokenString(token, false)}";
        }

        private string GetNextRandomTokenString(Token token, bool reverse, bool first = true)
        {
            if (token.ChildrenTokens == null) return "";

            var number = Utilities.Utilities.RandomZeroToNumberMinusOne(token.ChildrenTokens.Count);
            Token tempToken = TokenManager.GetTokenForID(token.ChildrenTokens[number]);

            if (first)
            {
                return GetNextRandomTokenString(tempToken, reverse, false);
            }
            else
            {
                if (reverse)
                {
                    return GetNextRandomTokenString(tempToken, reverse, false) + " " + tempToken.WordText;
                } else
                {
                    return tempToken.WordText + " " + GetNextRandomTokenString(tempToken, reverse, false);
                }
            }
        }
    }
}
