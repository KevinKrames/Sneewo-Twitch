using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneetoApplication.Data_Structures
{
    public class TwitchCredentials
    {
        public string twitchUsername { get; set; }
        public string twitchOAuth { get; set; }
        
        public TwitchCredentials()
        {
            dynamic data = Utilities.Utilities.loadJsonFile("twitch_credentials.json");

            if (data == null) return;

            twitchUsername = data.twitchUsername;
            twitchOAuth = data.twitchOAuth;
        }
    }
}
