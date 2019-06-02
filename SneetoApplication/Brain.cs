using SneetoApplication.Data_Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneetoApplication
{
    public class Brain
    {
        public static Dictionary<string, string> configuration;
        public static List<string> badWords;
        public static readonly string USE_DATABASE = "useDatabase";
        public static readonly string config = "configuration.json";
        public static readonly string badWordsFile = "badWords.txt";
        public static Form1 form;

        private TokenMemoryManager tokenMemoryManager;
        public Brain(Form1 form1)
        {
            form = form1;
            configuration = Utilities.Utilities.loadDictionaryFromJsonFile(config);
            badWords = Utilities.Utilities.loadListFromTextFile(badWordsFile);
            tokenMemoryManager = new TokenMemoryManager();
        }

        public string TimedGenerateSentence(string sourceSentence, int milisecondsToGenerate)
        {
            var result = "";

            var wordList = new TokenList(sourceSentence);

            tokenMemoryManager.UpdateUsedWords(wordList);

            var timeStarted = GetTimeMilliseconds();

            //while (GetTimeMilliseconds() - timeStarted < milisecondsToGenerate)
            //{
            //    result = GenerateRandomSentence(wordList);
            //}

            return result;
        }

        public int GetTimeMilliseconds() => (int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

        public string GenerateRandomSentence()
        {
            return GetNextRandomWord(tokenMemoryManager.GetForwardsRoot()).Trim();
        }

        private string GetNextRandomWord(Token token)
        {
            if (token.ChildrenTokens == null) return "";

            var number = Utilities.Utilities.RandomOneToNumber(token.ChildrenTokens.Count-1);
            Token tempToken = TokenManager.GetTokenForID(token.ChildrenTokens[number]);
            return tempToken.WordText + " " + GetNextRandomWord(tempToken);
        }
    }
}
