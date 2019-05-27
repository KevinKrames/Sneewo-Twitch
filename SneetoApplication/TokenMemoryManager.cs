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
            databaseManager = new DatabaseManager();
            InitializeMemory();
        }

        private void InitializeMemory()
        {
            if (Brain.configuration["useDatabase"] != null
                && Brain.configuration["useDatabase"].Equals("true", StringComparison.CurrentCultureIgnoreCase))
            {
                var data = databaseManager.RetrieveQueryString(QueryHolder.GetBackwardNodeRoot());
                data = databaseManager.RetrieveQueryString(QueryHolder.GetForwardNodeRoot());

            } else
            {
                forwardRoot = new Token();
                backwardRoot = new Token();
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
                    TrainTokenList(new TokenList(data));
                    if (linesTrained % 10000 == 0) { System.Console.WriteLine(@"Trained: {linesTrained}"); }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading file" + DATA_FILE_NAME);
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
            while (nextToken != null)
            {
                if (DoesTokenExist(nextToken.Current, currentToken, out var outIndex))
                {

                }

                nextToken.MoveNext();
            }
        }

        public bool DoesTokenExist(string nextToken, Token token, out int outIndex)
        {
            outIndex = -1;
            var children = token.ChildrenTokens;
            if (children == null && children.Count != 0) return false;

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
