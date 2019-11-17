using SneetoApplication.Data_Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneetoApplication
{
    public class ChannelMemory
    {
        public Dictionary<Stem, decimal> WordMemory;
        public ChannelMemory()
        {
            WordMemory = new Dictionary<Stem, decimal>();
        }

        internal void UpdateMessage(string message)
        {
            var majorWords = new TokenList(message).GetMajorWords();

            foreach(var word in majorWords)
            {
                var stem = StemManager.GetStemForToken(word);
                if (stem == null) continue;
                UpdateMemoryForStem(stem);
            }

            DecayMemory(0.0125m);
        }

        private void UpdateMemoryForStem(Stem stem)
        {
            var originalValue = 0m;
            if (!WordMemory.ContainsKey(stem)) {
                WordMemory[stem] = 1m;
                return;
            }
            originalValue = WordMemory[stem];
            var invertedValue = (1m - originalValue) < 0 ? 0 : (1m - originalValue);
            var bonusValue = 0.5m * invertedValue;
            if (invertedValue == 0)
            {
                WordMemory[stem] += 0.5m;
            } else
            {
                WordMemory[stem] = 1m + bonusValue;
            }
        }

        public void Update()
        {
            DecayMemory(0.025m);
        }

        private void DecayMemory(decimal value)
        {
            foreach (var stem in WordMemory.Keys.ToArray())
            {
                if (WordMemory[stem] >= 1m)
                {
                    WordMemory[stem] -= value + WordMemory[stem] * value;
                } else
                {
                    WordMemory[stem] -= value;
                }
                if (WordMemory[stem] <= 0.05m) WordMemory.Remove(stem);
            }
        }
    }
}
