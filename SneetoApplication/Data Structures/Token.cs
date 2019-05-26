using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneetoApplication.Data_Structures
{
    public class Token
    {
        private string wordText;
        private int ID;
        private int parentID;
        private int PMID;
        private int usage;
        private int childrenUsage;

        public Token() {}

        public void Increment()
        {
            usage++;
            //memoryManager.IncrementParentID(parentID);
        }
    }
}
