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
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure.Storage.Table.Protocol;

namespace ProductData
{

    public interface IImageStorage
    {
        IEnumerable<string> GetShufflesOlderThan(DateTime date);

        IEnumerable<Uri> GetAllShuffleParts(string shuffleId);

        Uri GetImageLink(string shuffleId);

        bool IsReadonly(string shuffleId);

        void AddNewPart(string shuffleId, string fileName, Stream fileStream);

        void RequestShuffle(string shuffleId);

        void Delete(string shuffleId);
    }

    public interface IProductStorage
    {
        IEnumerable<Product> GetAllProducts();
        IEnumerable<Product> GetProductsByStore(string storeName);
        IEnumerable<Product> GetProductsByStoreChain(string storeChainName, double salePercentage = 0.15);
        IEnumerable<Product> GetProductsByName(string productName);
        Product GetProductById(Guid id);
        IEnumerable<Product> GetProductsByCategory(Category category);
        IEnumerable<Product> GetProductsExpired();
        void Delete(IEnumerable<Product> products);
        bool AddProduct(Product p);
        Product GetProductByKeys(string primaryKey, string rowKey);
        void AddQueue(Product p);
        IEnumerable<Product> GetAllProductsToday();
    }

    public interface IProductUrlStorage
    {
        IEnumerable<ProductURL> GetAllProductUrls(bool active = true);
        IEnumerable<ProductURL> GetAllProductsByStore(string storeName, bool active = true);
        void AddProductUrl(ProductURL purl);

    }
    public static class StorageExtensions
    {
        public static bool SafeCreateIfNotExists(this CloudTable table, TableRequestOptions requestOptions = null, OperationContext operationContext = null)
        {
            do
            {
                try
                {
                    return table.CreateIfNotExists(requestOptions, operationContext);
                }
                catch (StorageException e)
                {
                    if ((e.RequestInformation.HttpStatusCode == 409) && (e.RequestInformation.ExtendedErrorInformation.ErrorCode.Equals(TableErrorCodeStrings.TableBeingDeleted)))
                        Thread.Sleep(1000);// The table is currently being deleted. Try again until it works.
                    else
                        throw;
                }
            } while (true);
        }
    }
    public class ProductStorage : IProductStorage
    {

        private readonly CloudStorageAccount storageAccount;
        private readonly CloudTableClient tableClient;
        private readonly CloudTable table;
        private readonly CloudQueue queue;



        public ProductStorage(string connectionString, string tableName)
            : this(CloudStorageAccount.Parse(connectionString), tableName)
        {
        }

        public ProductStorage(CloudStorageAccount storageAccount, string tableName)
        {
            this.storageAccount = storageAccount;
            this.tableClient = storageAccount.CreateCloudTableClient();
            this.table = tableClient.GetTableReference(tableName);
            this.queue = storageAccount.CreateCloudQueueClient().GetQueueReference("workerqueue");
            queue.CreateIfNotExists();
            table.SafeCreateIfNotExists();
        }

       public bool AddProduct(Product p)
        {
            p.RowKey = p.GetRowKey().ToString();
            p.PartitionKey = p.StoreChain;

            if (GetProductByKeys(p.PartitionKey, p.RowKey) == null)
            {

                TableOperation insertOperation = TableOperation.InsertOrReplace(p);
                table.Execute(insertOperation);
                return true;
            }

            return false;
        }

        public void AddQueue(Product p)
        {
            CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(p.PartitionKey + "," + p.RowKey);
            queue.AddMessage(cloudQueueMessage);
        }
        public IEnumerable<Product> GetAllProducts()
        {
            var results = (from entity in table.CreateQuery<Product>()
                           select entity).Take(100).ToList();

            return new List<Product>(results);
        }

        public IEnumerable<Product> GetProductsByStore(string storeName)
        {
            var results = (from entity in table.CreateQuery<Product>()
                           where string.Compare(entity.Store, storeName, true) == 0
                           select entity).ToList();

            return new List<Product>(results);
        }

        public IEnumerable<Product> GetProductsByStoreChain(string storeChainName, double salePercentage = 0.15)
        {
            IEnumerable<Product> results = null;

            //storeChainName = WebUtility.HtmlDecode(storeChainName);
            storeChainName = storeChainName.Replace("20%", " ").ToLower() ;
            /* rule to filter out products
             Costco: filter out 15% off above (by default)
             * Maycys: return top 20 most expensive ones
             * Outlet: all return
             */
            TableQuery<Product> query = new TableQuery<Product>().Where(TableQuery.GenerateFilterCondition("StoreChain", QueryComparisons.Equal, storeChainName));

            switch (storeChainName)
            {
                case "costco":
                    //results = (from entity in table.CreateQuery<Product>()
                    //           where (entity.PartitionKey.ToLower() == "costco"//storeChainName.ToLower()
                    //           && entity.SalePrice <= 100)//entity.OriginalPrice * (1 - salePercentage))
                    //           select entity).ToList();
                    results = table.ExecuteQuery(query).ToList();
                    results = results.Where(o => o.SalePrice <= o.OriginalPrice * (1 - salePercentage));
                    break;
                case "macys":
                    results = table.ExecuteQuery(query).ToList();
                    results = results.OrderByDescending(o => o.OriginalPrice).Take(20);
                    break;
                default:
                    //results = (from entity in table.CreateQuery<Product>()
                    //           where (entity.PartitionKey.ToLower() == "macys") //== storeChainName.ToLower())
                    //               select entity).ToList();

                    results = table.ExecuteQuery(query).ToList();
                    //results = results.Where(o => o.Store == o.OriginalPrice * (1 - salePercentage));
                    break;
            }

            return new List<Product>(results);
        }

        public IEnumerable<Product> GetProductsByName(string productName)
        {
            var results = from entity in table.CreateQuery<Product>()
                          where string.Compare(entity.ProductName, productName, true) == 0
                          select entity;

            return new List<Product>(results);
        }

        public Product GetProductById(Guid id)
        {
            var results = from entity in table.CreateQuery<Product>()
                          where entity.ProductId == id
                          select entity;

            return results.First();
        }

        public Product GetProductByKeys(string primaryKey, string rowKey)
        {
            var results = (from entity in table.CreateQuery<Product>()
                          where entity.PartitionKey == primaryKey && entity.RowKey == rowKey
                          select entity).ToList();

            return results.Count ==0 ?null:results.First();
        }

        public IEnumerable<Product> GetProductsByCategory(Category category)
        {
            var results = (from entity in table.CreateQuery<Product>()
                           where entity.Category == category.ToString()
                           select entity).Take(100).ToList();

            return new List<Product>(results);
        }


        public IEnumerable<Product> GetProductsExpired()
        {
            var results = (from entity in table.CreateQuery<Product>()
                           where Convert.ToDateTime(entity.CouponEndDate) <= DateTime.Now
                           select entity).Take(100).ToList();

            return new List<Product>(results);
        }

        public void Delete(IEnumerable<Product> products)
        {
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




        public IEnumerable<Product> GetAllProductsToday()
        {
            var results = (from entity in table.CreateQuery<Product>()
                           where entity.Timestamp >= DateTime.Today.AddDays(-7)
                           select entity).ToList().OrderByDescending(o => o.OriginalPrice).Take(10);

            return new List<Product>(results);
        }
    }

    public class ProductUrlStorage : IProductUrlStorage
    {
        private readonly CloudStorageAccount storageAccount;
        private readonly CloudTableClient tableClient;
        private readonly CloudTable table;

        public ProductUrlStorage(string connectionString)
            : this(CloudStorageAccount.Parse(connectionString))
        {
        }

        public ProductUrlStorage(CloudStorageAccount storageAccount)
        {
            this.storageAccount = storageAccount;
            this.tableClient = storageAccount.CreateCloudTableClient();
            this.table = tableClient.GetTableReference("ProductUrls");
        }


        public IEnumerable<ProductURL> GetAllProductUrls(bool active = true)
        {
            var results = from entity in table.CreateQuery<ProductURL>()
                          select entity;

            if (active)
                results = results.Where(o => o.IsActive == true);

            return new List<ProductURL>(results);
        }

        public IEnumerable<ProductURL> GetAllProductsByStore(string storeName, bool active = true)
        {
            var results = from entity in table.CreateQuery<ProductURL>()
                          where string.Compare(entity.StoreName, storeName, true) == 0
                          select entity;

            if (active)
                results = results.Where(o => o.IsActive == true);

            return new List<ProductURL>(results);
        }


        public void AddProductUrl(ProductURL purl)
        {
            purl.RowKey = purl.GetRowKey().ToString();
            purl.PartitionKey = purl.StoreName;

            TableOperation insertOperation = TableOperation.InsertOrReplace(purl);
            table.Execute(insertOperation);
        }
    }
}
