using System;
using System.Collections;

namespace MyDapper.DbCommon.Models
{
    public class Items
    {
        public Type Type { get; set; }

        public IEnumerable Enumerable { get; set; }

        public string[] Members { get; set; }
    }
}
