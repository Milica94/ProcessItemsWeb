using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class Article : TableEntity
    {
        public string Identifier { get; set; }
        public string Value { get; set; }

        public Article(string identifier)
        {
            PartitionKey = "Article";
            RowKey = identifier;
        }

        public Article() { }
    }
}
