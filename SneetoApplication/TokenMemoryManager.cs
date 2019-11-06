using SneetoApplication.Data_Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SneetoApplication
{
    public class TokenMemoryManager
    {
        private DatabaseManager databaseManager;
        private Token forwardsRoot;
        private Token backwardsRoot;
        private bool UseDatabase;

        public readonly static string DATA_FILE_NAME = "chatlog.json";

        public TokenMemoryManager()
        {
            SetupManager();
        }

        public virtual void SetupManager()
        {
            if (Brain.configuration[Brain.USE_DATABASE] != null
                && Brain.configuration[Brain.USE_DATABASE].Equals(bool.TrueString, StringComparison.CurrentCultureIgnoreCase))
            {
                databaseManager = new DatabaseManager();
            }
            InitializeMemory();
        }

        private void InitializeMemory()
        {
            if (Brain.configuration[Brain.USE_DATABASE] != null
                && Brain.configuration[Brain.USE_DATABASE].Equals(bool.TrueString, StringComparison.CurrentCultureIgnoreCase))
            {
                var data = databaseManager.RetrieveQueryString(QueryHolder.GetBackwardNodeRoot());
                data = databaseManager.RetrieveQueryString(QueryHolder.GetForwardNodeRoot());

            } else
            {
                forwardsRoot = TokenManager.TrainNewToken(null, "<start>", -1);
                forwardsRoot.reverse = false;
                backwardsRoot = TokenManager.TrainNewToken(null, "<end>", -1);
                backwardsRoot.reverse = true;
                TrainFromFile();
            }
        }

        private void TrainFromFile()
        {
            string data = "";
            try
            {
                int linesTrained = 0;
                StreamReader sr = new StreamReader(Path.GetDirectoryName(Application.ExecutablePath) + "\\files\\" + DATA_FILE_NAME);
                while (!sr.EndOfStream)
                {
                    data = sr.ReadLine();
                    if (data[data.Length-1] == ',')
                    {
                        data = data.Substring(0, data.Length - 1);
                    }

                    var message = new TokenList(Utilities.Utilities.jsonUnserialize(data)["message"]);
                    if (TrainSingleSentence(message)) linesTrained++;

                    if (linesTrained % 10000 == 0) {
                        Brain.form.consoleTextBox.AppendText($"Trained: {linesTrained}\n");
                        //if (linesTrained == 20000) break;
                    }
                }
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error training file" + DATA_FILE_NAME);
                Console.WriteLine(e.StackTrace);
            }
        }

        public bool TrainSingleSentence(TokenList tokenList)
        {
            if (!IsValidSentence(tokenList)) return false;
            TrainTokenList(tokenList);
            return true;
        }

        public bool IsValidSentence(TokenList tokenList)
        {
            foreach (var badWord in Brain.badWords)
            {
                if (badWord.Length == 0 || badWord[0] == '#') continue;

                if (badWord[0] == '*')
                {
                    var temp = tokenList.DoesContainAnyFormOfString(badWord.Substring(1));
                    if (temp) return false;
                }
                else if (tokenList.DoesContainWord(badWord))
                {
                    return false;
                }
            }

            return true;
        }

        public void TrainTokenList(TokenList tokenList)
        {
            var backwardsTokens = GetExisitingTokens(tokenList, GetForwardsRoot());
            tokenList.Invert();
            var forwardsTokens = GetExisitingTokens(tokenList, GetBackwardsRoot());

            if (forwardsTokens.Count == 0 || (backwardsTokens.Count != 0 && backwardsTokens.Count >= forwardsTokens.Count))
            {
                TrainTokenList(tokenList, GetBackwardsRoot(), null);
                var forwardsTokenLinks = GetExisitingTokens(tokenList, GetBackwardsRoot());
                tokenList.Invert();
                TrainTokenList(tokenList, GetForwardsRoot(), backwardsTokens, forwardsTokenLinks);
            }
            else
            {
                tokenList.Invert();
                TrainTokenList(tokenList, GetForwardsRoot(), null);
                var backwardsTokenLinks = GetExisitingTokens(tokenList, GetForwardsRoot());
                tokenList.Invert();
                TrainTokenList(tokenList, GetBackwardsRoot(), forwardsTokens, backwardsTokenLinks);
            }
        }

        public List<Token> GetExisitingTokens(TokenList tokenList, Token token)
        {
            var existingTokens = new List<Token>();
            var nextToken = tokenList.GetEnumerator();
            var hasNextToken = nextToken.MoveNext();
            while (hasNextToken)
            {
                if (TokenManager.DoesWordTextExist(nextToken.Current, token, out var outIndex))
                {
                    token = TokenManager.GetTokenForID(token.ChildrenTokens[outIndex]);
                    //Insert at front of list since the reverse training will need to be inverted
                    existingTokens.Insert(0, token);
                } else
                {
                    break;
                }
                hasNextToken = nextToken.MoveNext();
            }
            return existingTokens;
        }

        public void TrainTokenList(TokenList tokenList, Token currentToken, List<Token> existingTokens, List<Token> linkedTokens = null)
        {
            var tokenListTotal = tokenList.Get().Count;
            var currentTokenCounter = 0;
            var nextToken = tokenList.GetEnumerator();
            var hasNextToken = nextToken.MoveNext();
            while (hasNextToken)
            {
                currentTokenCounter++;
                if (TokenManager.DoesWordTextExist(nextToken.Current, currentToken, out var outIndex))
                {
                    currentToken = TokenManager.TrainExistingToken(currentToken, outIndex);

                    if (existingTokens != null && existingTokens.Contains(currentToken))
                    {
                        existingTokens = null;
                    }
                }
                else
                {
                    if (existingTokens != null
                        && existingTokens.Count > 0
                        && (existingTokens.Count + currentTokenCounter-1) == tokenListTotal
                        && existingTokens[0].WordText.Equals(nextToken.Current))
                    {
                        currentToken = TokenManager.TrainReferenceExistingToken(currentToken, existingTokens[0], outIndex);
                        existingTokens.RemoveAt(0);
                    }
                    else
                    {
                        currentToken = TokenManager.TrainNewToken(currentToken, nextToken.Current, outIndex);
                    }
                }
                if (linkedTokens?.Count > 0)
                    TokenManager.LinkTokensAndRemoveFirstItem(currentToken, linkedTokens);
                hasNextToken = nextToken.MoveNext();
            }
        }

        public Token GetForwardsRoot()
        {
            return forwardsRoot;
        }

        public Token GetBackwardsRoot()
        {
            return backwardsRoot;
        }

        public void SetForwardsRoot(Token token)
        {
            forwardsRoot = token;
        }

        public void SetBackwardsRoot(Token token)
        {
            backwardsRoot = token;
        }

        internal void UpdateUsedWords(TokenList wordList)
        {
            throw new NotImplementedException();
        }

        internal void IncrementParentID(int parentID)
        {
            throw new NotImplementedException();
        }

        internal Token CreateOrGetNode(string word, Token lastNode)
        {
            throw new NotImplementedException();
        }

        public List<Token> getChildNodes(Token currentNode)
        {
            throw new NotImplementedException();
        }
    }
}
