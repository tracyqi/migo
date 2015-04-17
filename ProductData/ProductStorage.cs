using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Queue.Protocol;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;

namespace ProductData
{
 
    public class ProductStorage : IProductStorage
    {

        private readonly CloudStorageAccount storageAccount;
        private readonly CloudTableClient tableClient;
        private readonly CloudBlobClient blobClient;
        private readonly CloudQueueClient queueClient;


        public ProductStorage(string connectionString)
            : this(CloudStorageAccount.Parse(connectionString))
        {
        }

        public ProductStorage(CloudStorageAccount storageAccount)
        {
            this.storageAccount = storageAccount;
            this.tableClient = storageAccount.CreateCloudTableClient();
            this.blobClient = this.storageAccount.CreateCloudBlobClient();
            this.queueClient = this.storageAccount.CreateCloudQueueClient();
        }

        public IEnumerable<Product> GetAllProducts()
        {
            CloudTable table = tableClient.GetTableReference("Products");

            var results = (from entity in table.CreateQuery<Product>()
                           //where entity.PartitionKey == "奥特莱"
                           select entity).Take(100).ToList();

            return new List<Product>(results);
        }

        public IEnumerable<Product> GetAllProductsByStore(string storeName)
        {
            CloudTable table = tableClient.GetTableReference("Products");

            var results = (from entity in table.CreateQuery<Product>()
                           where string.Compare(entity.PartitionKey, storeName, true) ==0
                           select entity).Take(100).ToList();

            return new List<Product>(results);
        }

        public IEnumerable<Product> GetProductsByName(string productName)
        {
            CloudTable table = tableClient.GetTableReference("Products");

            var results = from entity in table.CreateQuery<Product>()
                          where string.Compare(entity.ProductName, productName, true) == 0
                          select entity;

            return new List<Product>(results);
        }

        public Product GetProductsById(Guid id)
        {
            CloudTable table = tableClient.GetTableReference("Products");

            var results = from entity in table.CreateQuery<Product>()
                           where entity.ProductId== id
                           select entity;

            return results.First();
        }

        public IEnumerable<Product> GetProductsByCategory(Category category)
        {
            CloudTable table = tableClient.GetTableReference("Products");

            var results = (from entity in table.CreateQuery<Product>()
                           where entity.Category == category
                           select entity).Take(100).ToList();

            return new List<Product>(results);
        }


        public IEnumerable<Product> GetProductsExpired()
        {
            CloudTable table = tableClient.GetTableReference("Products");

            var results = (from entity in table.CreateQuery<Product>()
                           where entity.CouponEndDate  <= DateTime.Now
                           select entity).Take(100).ToList();

            return new List<Product>(results);
        }

        public void Delete(IEnumerable<Product> products)
        {
            CloudTable table = tableClient.GetTableReference("Products");

            foreach (var r in products)
            {
                var results = (from entity in table.CreateQuery<Product>()
                               where entity.ProductId == r.ProductId
                               select entity).Take(100).ToList();

                TableOperation deleteOperation = TableOperation.Delete(r); 
                // Execute the operation. 
                table.Execute(deleteOperation); 
        }

        }
    }
}
