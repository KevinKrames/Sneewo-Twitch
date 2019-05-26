using SneetoApplication.Data_Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneetoApplication
{
    public class TokenMemoryManager
    {
        private DatabaseManager databaseManager;
        private Token forwardRoot;
        private Token backwardRoot;
        private bool UseDatabase;

        public TokenMemoryManager()
        {
            databaseManager = new DatabaseManager();
            InitializeMemory();
        }

        private void InitializeMemory()
        {
            forwardRoot = new Token();
            backwardRoot = new Token();
        }

        public Token GetForwardsTree()
        {
            var data = databaseManager.RetrieveQueryString(QueryHolder.GetForwardNodeRoot());
            return forwardRoot;
        }
        
        public Token GetBackwardsTree()
        {
            var data = databaseManager.RetrieveQueryString(QueryHolder.GetBackwardNodeRoot());
            return backwardRoot;
        }

        internal void UpdateUsedWords(WordList wordList)
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
