using Annytab.Stemmer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneetoApplication
{
    public class WordStemmer
    {
        private static IStemmer stemmer;
        protected static IStemmer StemmerCreator {
            get
            {
                if (stemmer == null)
                {
                    stemmer = new EnglishStemmer();
                }
                return stemmer;
            }
        }

        public static string StemifyWord(string word)
        {
            return StemmerCreator.GetSteamWord(word);
        }
    }
}
