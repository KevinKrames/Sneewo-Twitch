using Annytab.Stemmer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SneetoApplication.Data_Structures
{
    public static class StemManager
    {
        public static Dictionary<string, Stem> StemDictionary = new Dictionary<string, Stem>();
        public static IStemmer Stemmer = new EnglishStemmer();
        public static Regex specialCharactersRegex = new Regex("[^a-zA-Z0-9 -]");

        public static void AddToken(Token token)
        {
            var filteredText = specialCharactersRegex.Replace(token.WordText, "").ToLower();
            var stemText = Stemmer.GetSteamWord(filteredText);
            if (StemDictionary.TryGetValue(stemText, out Stem stem))
            {
                stem.stemTokens.Add(token.ID);
            }
            else
            {
                StemDictionary[stemText] = new Stem();
                StemDictionary[stemText].stemText = stemText;
                StemDictionary[stemText].stemTokens = new List<Guid>();
                StemDictionary[stemText].stemTokens.Add(token.ID);
            }
        }
    }
}
