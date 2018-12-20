using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MyDapper.Model
{
    public class BulkItem
    {
        public string ConnectionString { get; set; }


        public string BatchSize { get; set; }

        public Dictionary<string, Items> Items { get; set; }
    }

    public class Items
    {
        public Type Type { get; set; }

        public IEnumerable Enumerable { get; set; }

        public string[] Members { get; set; }
    }
}
