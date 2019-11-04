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

        public static Token TrainExistingToken(Token parentToken, int index)
        {
            var token = GetTokenForID(parentToken.ChildrenTokens[index]);
            return token;
        }

        public static Token TrainNewToken(Token parentToken, string newTokenText, int index)
        {
            if (Brain.configuration[Brain.USE_DATABASE].Equals(bool.FalseString, StringComparison.CurrentCultureIgnoreCase))
            {
                var token = new Token();
                token.ID = Guid.NewGuid();
                token.WordText = newTokenText;
                if (parentToken != null) token.reverse = parentToken.reverse;
                SetTokenForID(token.ID, token);

                if (!token.WordText.Equals("<start>") && !token.WordText.Equals("<end>"))
                    StemManager.AddToken(token);

                if (parentToken != null)
                {
                    if (index < 0)
                    {
                        parentToken.ChildrenTokens = new List<Guid>();
                        index = 0;
                    }
                    parentToken.ChildrenTokens.Insert(index, token.ID);
                }
                return token;
            }
            return null;
        }

        public static bool DoesWordTextExist(string nextToken, Token token, out int index)
        {
            var returnValue = DoesTokenExist(nextToken, token.ChildrenTokens, out index);
            return returnValue;
        }

        public static bool DoesTokenExist(string nextToken, List<Guid> guidList, out int outIndex)
        {
            outIndex = -1;
            if (guidList == null || guidList.Count == 0) return false;

            var startSearchIndex = 0;
            var currentSearchIndex = 0;
            var maxSearchIndex = guidList.Count;
            Token currentToken;

            while (startSearchIndex != maxSearchIndex)
            {
                currentSearchIndex = (maxSearchIndex - startSearchIndex) / 2;
                currentToken = TokenManager.GetTokenForID(guidList[currentSearchIndex + startSearchIndex]);
                if (currentToken.WordText.Equals(nextToken))
                {
                    outIndex = currentSearchIndex + startSearchIndex;
                    return true;
                }

                if (String.Compare(currentToken.WordText, nextToken) > 0)
                {
                    maxSearchIndex = currentSearchIndex + startSearchIndex;
                }
                else
                {
                    if (currentSearchIndex == 0)
                    {
                        currentSearchIndex++;
                        break;
                    }
                    startSearchIndex = currentSearchIndex + startSearchIndex;
                }
            }
            outIndex = currentSearchIndex + startSearchIndex;
            return false;
        }

        internal static void LinkTokensAndRemoveFirstItem(Token currentToken, List<Token> linkedTokens)
        {
            var partner = linkedTokens[0];
            currentToken.PartnerID = partner.ID;
            partner.PartnerID = currentToken.ID;
            linkedTokens.RemoveAt(0);
        }

        public static Token TrainReferenceExistingToken(Token parentToken, Token existingToken, int outIndex)
        {
            if (parentToken.ChildrenTokens == null || parentToken.ChildrenTokens.Count == 0)
            {
                parentToken.ChildrenTokens = new List<Guid>();
                outIndex = 0;
            }
            parentToken.ChildrenTokens.Insert(outIndex, existingToken.ID);
            return existingToken;
        }
    }
}
