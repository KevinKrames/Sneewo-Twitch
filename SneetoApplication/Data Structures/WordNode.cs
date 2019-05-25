using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneetoApplication.Data_Structures
{
    public class WordNode
    {
        private string wordText;
        private int ID;
        private int parentID;
        private int PMID;
        private int usage;
        private int childrenUsage;
        private MemoryManager memoryManager;

        public WordNode(MemoryManager manager)
        {
            memoryManager = manager;
        }

        public void Increment()
        {
            usage++;
            memoryManager.IncrementParentID(parentID);
        }
    }
}
