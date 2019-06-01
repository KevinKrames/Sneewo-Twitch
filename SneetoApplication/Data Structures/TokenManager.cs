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

        public static void ClearTokens()
        {
            TokenDictionary = new Dictionary<Guid, Token>();
        }

        public static Token TrainExistingToken(Token Parent, int index)
        {
            var token = GetTokenForID(Parent.ChildrenTokens[index]);
            return token;
        }

        public static Token TrainNewToken(Token currentToken, string newTokenText, int index)
        {
            if (Brain.configuration[Brain.USE_DATABASE].Equals(bool.FalseString, StringComparison.CurrentCultureIgnoreCase))
            {
                var token = new Token();
                token.ID = Guid.NewGuid();
                token.ParentID = currentToken != null ? currentToken.ID : new Guid();
                token.WordText = newTokenText;
                SetTokenForID(token.ID, token);

                if (!token.WordText.Equals("<start>") && !token.WordText.Equals("<end>"))
                    StemManager.AddToken(token);

                if (currentToken != null)
                {
                    if (index < 0)
                    {
                        currentToken.ChildrenTokens = new List<Guid>();
                        index = 0;
                    }
                    currentToken.ChildrenTokens.Insert(index, token.ID);
                }
                return token;
            }
            return null;
        }
    }
}
