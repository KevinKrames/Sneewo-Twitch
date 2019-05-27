﻿using SneetoApplication.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneetoApplication.Data_Structures
{
    public class TokenList : IEnumerable<string>
    {
        private List<string> wordList;
        public TokenList()
        {
            wordList = new List<string>();
        }

        public TokenList(string sentence)
        {
            wordList = SplitBySpace(sentence);
        }

        public List<string> SplitBySpace(string sentence)
        {
            return sentence.Trim().Split(' ').ToList();
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

        public IEnumerator<string> GetEnumerator()
        {
            foreach (var word in wordList)
            {
                yield return word;
            }
            yield return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}