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
        private List<ChannelMemorySentence> memorySentences;
        private static int MaxSentenceDuration = 60000;
        
        public ChannelMemory()
        {
            WordMemory = new Dictionary<Stem, decimal>();
            memorySentences = new List<ChannelMemorySentence>();
        }

        public List<ChannelMemorySentence> GetMemorySentences()
        {
            return memorySentences;
        }

        internal void UpdateMessage(string message)
        {
            var majorWords = new TokenList(message).GetMajorWords().Distinct();

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
            var bonusValue = 2m * (invertedValue * 0.5m);
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
            UpdateMemorySentences();
            DecayMemory(0.025m);
        }

        private void UpdateMemorySentences()
        {
            var deleteSentences = new List<ChannelMemorySentence>();
            foreach (var sentence in memorySentences)
            {
                if (GetTimeMilliseconds() - sentence.TimeSent > MaxSentenceDuration)
                {
                    deleteSentences.Add(sentence);
                }
            }
            
            foreach (var sentence in deleteSentences) { memorySentences.Remove(sentence); }
        }

        public virtual void DecayMemory(decimal value)
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

        public bool HasMessageSent(string message)
        {
            bool result = true;
            foreach (var sentence in memorySentences)
            {
                if (sentence.Text == message)
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        public void MessageSent(string message)
        {
            memorySentences.Add(new ChannelMemorySentence
            {
                Text = message,
                TimeSent = GetTimeMilliseconds()
            });
        }

        public virtual int GetTimeMilliseconds() => (int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
    }
}
