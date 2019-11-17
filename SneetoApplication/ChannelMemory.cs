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
        private Dictionary<Stem, decimal> wordMemory;
        public ChannelMemory()
        {
            wordMemory = new Dictionary<Stem, decimal>();
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
        }

        private void UpdateMemoryForStem(Stem stem)
        {
            DecayMemory(0.05m);
            var originalValue = 0m;
            if (!wordMemory.ContainsKey(stem)) {
                wordMemory[stem] = 1m;
                return;
            }
            originalValue = wordMemory[stem];
            var invertedValue = (1m - originalValue) < 0 ? 0 : (1m - originalValue);
            var bonusValue = 0.25m * invertedValue;
            if (invertedValue == 0)
            {
                wordMemory[stem] += 0.25m;
            } else
            {
                wordMemory[stem] = 1m + bonusValue;
            }
        }

        public void Update()
        {
            DecayMemory(0.1m);
        }

        private void DecayMemory(decimal value)
        {
            foreach (var stem in wordMemory.Keys)
            {
                if (wordMemory[stem] >= 1m)
                {
                    wordMemory[stem] -= value + wordMemory[stem] * value;
                } else
                {
                    wordMemory[stem] -= value;
                }
            }
        }
    }
}
