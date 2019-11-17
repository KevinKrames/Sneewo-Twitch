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

        public static void ClearStems()
        {
            StemDictionary = new Dictionary<string, Stem>();
        }

        public static string GetStemTextForToken(string text)
        {
            var filteredText = specialCharactersRegex.Replace(text, "").ToLower();
            return Stemmer.GetSteamWord(filteredText);
        }

        public static Stem GetStemForToken(string text)
        {
            var filteredText = specialCharactersRegex.Replace(text, "").ToLower();
            if (filteredText != null && !StemDictionary.ContainsKey(Stemmer.GetSteamWord(filteredText))) return null;
            return StemDictionary[Stemmer.GetSteamWord(filteredText)];
        }

        public static void AddToken(Token token)
        {
            var stemText = GetStemTextForToken(token.WordText);
            if (StemDictionary.TryGetValue(stemText, out Stem stem))
            {
                if (!stem.stemTokens.Contains(token.ID))
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

        public static List<Guid> GetGuidsForUnstemmedWord(string word)
        {
            var stemText = GetStemTextForToken(word);
            if (StemDictionary.TryGetValue(stemText, out Stem stem))
            {
                if (stem.stemTokens != null && stem.stemTokens.Count > 0)
                    return stem.stemTokens;
            }
            return null;
        }

        public static List<Token> GetTokensForUnstemmedWord(string word)
        {
            var guids = GetGuidsForUnstemmedWord(word);
            if (guids == null) return null;

            return guids.Select(g => TokenManager.GetTokenForID(g)).ToList();
        }

        internal static string GetRandomStemWord()
        {
            var maxCount = StemDictionary.Keys.Count;
            var randomText = StemDictionary.Keys.ElementAt(Utilities.Utilities.RandomOneToNumber(maxCount) - 1);
            var tokens = GetTokensForUnstemmedWord(randomText);
            if (tokens == null || tokens.Count == 0) return null;
            return tokens[Utilities.Utilities.RandomOneToNumber(tokens.Count) - 1].WordText;
        }
    }
}
