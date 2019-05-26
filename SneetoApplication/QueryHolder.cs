using SneetoApplication.Data_Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneetoApplication
{
    // Let's go with raw queries instead of using EntityFramework. This should improve optimization since we can optimize queries.
    public static class QueryHolder
    {
        public static string GetForwardNodeRoot()
        {
            return
                "SELECT * FROM WordNode WHERE WordText = 'Forward' AND parentID IS NULL";
        }

        internal static string GetBackwardNodeRoot()
        {
            return
                "SELECT * FROM WordNode WHERE WordText = 'Backward' AND parentID IS NULL";
        }
    }
}
