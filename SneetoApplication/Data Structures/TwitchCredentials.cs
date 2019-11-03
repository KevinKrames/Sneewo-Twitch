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
        private static TwitchCredentials twitchCredentials;

        public static TwitchCredentials Instance
        {
            get
            {
                if (twitchCredentials == null)
                {
                    twitchCredentials = new TwitchCredentials();
                }
                return twitchCredentials;
            }
            set
            {
                twitchCredentials = value;
            }
        }

        public TwitchCredentials()
        {
            var data = Utilities.Utilities.loadDictionaryFromJsonFile("twitch_credentials.json");

            if (data == null) return;

            twitchUsername = data["twitchUsername"];
            twitchOAuth = data["twitchOAuth"];
        }
    }
}
