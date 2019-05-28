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
        public static readonly string USE_DATABASE = "useDatabase";
        public static Form1 form;

        private TokenMemoryManager tokenMemoryManager;
        public Brain(Form1 form1)
        {
            form = form1;
            configuration = Utilities.Utilities.loadDictionaryFromJsonFile("configuration.json");
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

        public void TrainSentence(string sourceSentence)
        {
            var wordList = new TokenList(sourceSentence);
            TrainSentenceTree(wordList.Get(), tokenMemoryManager.GetForwardsTree());
            wordList.Invert();
            TrainSentenceTree(wordList.Get(), tokenMemoryManager.GetBackwardsTree());
        }

        private void TrainSentenceTree(List<string> sourceSentence, Token tree)
        {
            var currentNode = tree;
            Token lastNode = null;
            foreach(string word in sourceSentence)
            {
                if (lastNode != null)
                {
                    currentNode = tokenMemoryManager.CreateOrGetNode(word, lastNode);
                }

                //currentNode.Increment();

                lastNode = currentNode;
            }
        }
    }
}
