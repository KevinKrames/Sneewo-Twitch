using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneetoApplication.Data_Structures
{
    public static class TokenManager
    {
        public static Dictionary<Guid, Token> TokenDictionary = new Dictionary<Guid, Token>();
        public static Token GetTokenForID(Guid id)
        {
            return TokenDictionary[id];
        }

        public static void SetTokenForID(Guid id, Token token)
        {
            TokenDictionary[id] = token;
        }
    }
}
