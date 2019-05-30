﻿using SneetoApplication.Data_Structures;
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

            while (GetTimeMilliseconds() - timeStarted < milisecondsToGenerate)
            {
                result = GenerateRandomSentence(wordList);
            }

            return result;
        }

        private string GenerateRandomSentence(TokenList wordList)
        {
            var sentence = new TokenList();

            var currentNode = Utilities.Utilities.RandomOneToNumber(2) == 1 ? tokenMemoryManager.GetForwardsTree() : tokenMemoryManager.GetBackwardsTree();

            var childNodes = tokenMemoryManager.getChildNodes(currentNode);

            return sentence.GetString();
        }

        public int GetTimeMilliseconds() => (int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

        public string GenerateRandomSentence()
        {
            return GetNextRandomWord(tokenMemoryManager.GetForwardsTree()).Trim();
        }

        private string GetNextRandomWord(Token token)
        {
            if (token.ChildrenTokens == null) return "";

            var number = Utilities.Utilities.RandomOneToNumber(token.TotalChildrenUsage);
            Token tempToken;

            foreach(Guid guid in token.ChildrenTokens)
            {
                tempToken = TokenManager.GetTokenForID(guid);
                number -= tempToken.Usage;
                if (number <= 0)
                {
                    return tempToken.WordText + " " + GetNextRandomWord(tempToken);
                }
            }
            return "ERROR";
        }
    }
}
