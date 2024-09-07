using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneetoApplication.Data_Structures
{
    public class Episode
    {
        public Queue<VoiceLine> Script;
        public Queue<VoiceLine> VoiceLinesToCleanup;
        public string fileLocation;
        public bool complete;
        public bool isPlaying;
        public bool played;
        public bool success = true;
        public string directoryName;
        public string User;
        public string Request;

        public Episode()
        {
            complete = false;
            isPlaying = false;
            played = false;
            Script = new Queue<VoiceLine>();
            VoiceLinesToCleanup = new Queue<VoiceLine>();
        }
    }
}
