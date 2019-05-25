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
        private MemoryManager memoryManager;
        private const float TicksToMilliseconds = 10000;
        public Brain()
        {
            memoryManager = new MemoryManager();
        }

        public string TimedGenerateSentence(string sourceSentence, int milisecondsToGenerate)
        {
            var result = "";

            var wordList = new WordList(sourceSentence);

            memoryManager.UpdateUsedWords(wordList);

            var timeStarted = GetTimeMilliseconds();

            while (GetTimeMilliseconds() - timeStarted < milisecondsToGenerate)
            {
                result = GenerateRandomSentence(wordList);
            }

            return result;
        }

        private string GenerateRandomSentence(WordList wordList)
        {
            var sentence = new WordList();

            var currentNode = Utilities.Utilities.RandomOneToNumber(2) == 1 ? memoryManager.GetForwardsTree() : memoryManager.GetBackwardsTree();

            var childNodes = memoryManager.getChildNodes(currentNode);

            return sentence.GetString();
        }

        public int GetTimeMilliseconds() => (int)(DateTime.Now.Ticks / TicksToMilliseconds);

        public void TrainSentence(string sourceSentence)
        {
            var wordList = new WordList(sourceSentence);
            TrainSentenceTree(wordList.Get(), memoryManager.GetForwardsTree());
            wordList.Invert();
            TrainSentenceTree(wordList.Get(), memoryManager.GetBackwardsTree());
        }

        private void TrainSentenceTree(List<string> sourceSentence, WordNode tree)
        {
            var currentNode = tree;
            WordNode lastNode = null;
            foreach(string word in sourceSentence)
            {
                if (lastNode != null)
                {
                    currentNode = memoryManager.CreateOrGetNode(word, lastNode);
                }

                currentNode.Increment();

                lastNode = currentNode;
            }
        }
    }
}
