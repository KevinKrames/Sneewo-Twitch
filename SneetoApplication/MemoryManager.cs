using SneetoApplication.Data_Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneetoApplication
{
    public class MemoryManager
    {
        private DatabaseManager databaseManager;
        private WordNode forwardRoot;
        private WordNode backwardRoot;

        public MemoryManager()
        {
            databaseManager = new DatabaseManager();
        }

        public WordNode GetForwardsTree()
        {
            var data = databaseManager.RetrieveQueryString(QueryHolder.GetForwardNodeRoot());
            return forwardRoot;
        }

        internal void UpdateUsedWords(WordList wordList)
        {
            throw new NotImplementedException();
        }

        internal void IncrementParentID(int parentID)
        {
            throw new NotImplementedException();
        }

        public WordNode GetBackwardsTree()
        {
            var data = databaseManager.RetrieveQueryString(QueryHolder.GetBackwardNodeRoot());
            return backwardRoot;
        }

        internal WordNode CreateOrGetNode(string word, WordNode lastNode)
        {
            throw new NotImplementedException();
        }

        public List<WordNode> getChildNodes(WordNode currentNode)
        {
            throw new NotImplementedException();
        }
    }
}
