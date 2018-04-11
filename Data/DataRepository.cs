using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class DataRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public DataRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("ArticleTable");
            _table.CreateIfNotExists();
        }

        public void AddArticle (Article article)
        {
            TableOperation insertOperation = TableOperation.Insert(article);
            _table.Execute(insertOperation);
        }

        public Article GetArticle (string identifier)
        {
            var result =  (from a in _table.CreateQuery<Article>() where a.RowKey == identifier select a).FirstOrDefault();
            return result;
        }
    }
}
