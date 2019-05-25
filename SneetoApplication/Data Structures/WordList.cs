using SneetoApplication.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneetoApplication.Data_Structures
{
    public class WordList
    {
        private List<string> wordList;
        public WordList()
        {
            wordList = new List<string>();
        }

        public WordList(string sentence)
        {
            wordList = SplitBySpace(sentence);
        }

        public List<string> SplitBySpace(string sentence)
        {
            sentence = sentence.Trim();
            var data = new List<string>();
            string[] words = sentence.Split(null);

            foreach (var word in words)
            {
                data.Add(word);
            }

            return data;
        }

        public void Invert() => wordList.Reverse();

        public List<string> Get()
        {
            return wordList;
        }

        public string GetString()
        {
            return String.Join(" ", wordList);
        }

        public void Append(string word)
        {
            wordList.Add(word);
        }
    }
}
