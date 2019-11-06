using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneetoApplication
{
    public class Channel
    {
        public string name;
        public bool mute;
        public int frequency;
        private int timeSinceLastSpeak;
        public Channel(string name)
        {
            this.name = name;
            mute = false;
            frequency = 30;
            timeSinceLastSpeak = 0;
        }

        public void Update()
        {
            if (timeSinceLastSpeak > 0) timeSinceLastSpeak--;
        }

        public bool CanSpeak() { return timeSinceLastSpeak == 0; }

        internal void SetSpeakTime()
        {
            timeSinceLastSpeak = frequency;
        }
    }
}
