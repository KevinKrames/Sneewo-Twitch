using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneetoApplication.Data_Structures
{
    public class VoiceLine
    {
        public string Character;
        public string Voice;
        public string Message;
        public string Number;
        public string guid;
        public bool downloaded;
        public bool complete;
        public bool success = true;
        public int retryCounter = 5;
        public VoiceLine()
        {
            complete = false;
            downloaded = false;
            guid = Guid.NewGuid().ToString();
        }
    }
}
