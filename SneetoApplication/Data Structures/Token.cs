using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneetoApplication.Data_Structures
{
    public class Token
    {
        public string WordText;
        public Guid ID;
        public Guid StemID;
        public Guid PartnerID;
        public List<Guid> ChildrenTokens;
    }
}
