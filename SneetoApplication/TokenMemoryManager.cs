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
        private Token forwardRoot;
        private Token backwardRoot;
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
                forwardRoot = TokenManager.TrainNewToken(null, "<start>", -1);
                backwardRoot = TokenManager.TrainNewToken(null, "<end>", -1);
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
                    linesTrained++;
                    data = sr.ReadLine();
                    if (data[data.Length-1] == ',')
                    {
                        data = data.Substring(0, data.Length - 1);
                    }
                    TrainTokenList(new TokenList(Utilities.Utilities.jsonUnserialize(data)["message"]));
                    if (linesTrained % 10000 == 0) { Brain.form.consoleTextBox.AppendText($"Trained: {linesTrained}\n"); }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error training file" + DATA_FILE_NAME);
                Console.WriteLine(e.StackTrace);
            }
        }

        private void TrainTokenList(TokenList tokenList)
        {
            TrainTokenList(tokenList, GetForwardsTree());
            tokenList.Invert();
            TrainTokenList(tokenList, GetBackwardsTree());
        }

        private void TrainTokenList(TokenList tokenList, Token currentToken)
        {
            var nextToken = tokenList.GetEnumerator();
            var hasNextToken = nextToken.MoveNext();
            while (hasNextToken)
            {
                if (DoesTokenExist(nextToken.Current, currentToken, out var outIndex))
                {
                    TokenManager.TrainExistingToken(currentToken, outIndex);
                }
                else
                {
                    TokenManager.TrainNewToken(currentToken, nextToken.Current, outIndex);
                }
                hasNextToken = nextToken.MoveNext();
            }
        }

        public bool DoesTokenExist(string nextToken, Token token, out int outIndex)
        {
            outIndex = -1;
            var children = token.ChildrenTokens;
            if (children == null || children.Count == 0) return false;

            var startSearchIndex = 0;
            var currentSearchIndex = 0;
            var maxSearchIndex = children.Count;
            Token currentToken;

            while (startSearchIndex != maxSearchIndex)
            {
                currentSearchIndex = (maxSearchIndex - startSearchIndex) / 2;
                currentToken = TokenManager.GetTokenForID(children[currentSearchIndex + startSearchIndex]);
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

        public Token GetForwardsTree()
        {
            return forwardRoot;
        }
        
        public Token GetBackwardsTree()
        {
            return backwardRoot;
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
