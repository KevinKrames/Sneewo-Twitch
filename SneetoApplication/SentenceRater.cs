using SneetoApplication.Data_Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneetoApplication
{
    public class SentenceRater
    {
        private static SentenceRater sentenceRater;

        public static SentenceRater Instance
        {
            get
            {
                if (sentenceRater == null)
                {
                    sentenceRater = new SentenceRater();
                }
                return sentenceRater;
            }
            set
            {
                sentenceRater = value;
            }
        }

        public decimal GetRatingForSentence(string channel, string sentence)
        {
            var rating = 0m;
            var tokenSentence = new TokenList(sentence);

            var nextToken = tokenSentence.GetEnumerator();
            var hasNextToken = nextToken.MoveNext();
            var usedStems = new List<Stem>();

            while (hasNextToken)
            {
                var stem = StemManager.GetStemForToken(nextToken.Current);
                if (stem != null
                    && ChannelMemoryManager.Instance.HasStemInChannel(channel, stem)
                    && !usedStems.Contains(stem))
                        rating += ChannelMemoryManager.Instance.GetValueForStem(channel, stem);
                hasNextToken = nextToken.MoveNext();
            }

            if (tokenSentence.Get().Count == 0) return rating;

            var power = Math.Pow(tokenSentence.Get().Count, 0.2);
            var multiplier = (decimal)(1 / power);
            rating = rating * multiplier;
            return rating;
        }
    }
}
