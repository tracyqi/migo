using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using ProductData;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace ProductFetcher
{
    class Program
    {
        static void Main(string[] args)
        {
            //JobHost jobHost = new JobHost();
            //jobHost.RunAndBlock();

            //GenerateProductUrlsTable();

            ProductFetcherJob j = new ProductFetcherJob();
            j.FetchData();
        }

        // This is a temporary function to generate producturl table
        private static void GenerateProductUrlsTable()
        {
            string conn = ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString;
            IProductUrlStorage productUrlStorage = new ProductUrlStorage(conn);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(conn);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the table if it doesn't exist. 
            CloudTable table = tableClient.GetTableReference("ProductUrls");
            table.CreateIfNotExistsAsync();

            ProductURL p = new ProductURL();
            p.Category = Category.Health.ToString();
            p.StoreName = "Costco";
            p.ProductUrl = "http://www.costco.com/adult-multi-letter-vitamins.html?pageSize=96";
            p.IsActive = true;
            p.PartitionKey = p.StoreName;
            p.RowKey = Guid.NewGuid().ToString();

            TableOperation insertOperation = TableOperation.Insert(p);
            // Execute the insert operation. 
            table.Execute(insertOperation);


            p = new ProductURL();
            p.Category = Category.Beauty.ToString();
            p.StoreName = "Costco";
            p.ProductUrl = "http://www.costco.com/skin-care.html?pageSize=96";
            p.IsActive = true;
            p.PartitionKey = p.StoreName;
            p.RowKey = Guid.NewGuid().ToString();

            insertOperation = TableOperation.Insert(p);
            // Execute the insert operation. 
            table.Execute(insertOperation);

            p = new ProductURL();
            p.Category = Category.Beauty.ToString();
            p.StoreName = "Macys";
            p.ProductUrl = "http://www1.macys.com/shop/makeup-and-perfume/gift-sets?id=55537&edge=hybrid";
            p.IsActive = true;
            p.PartitionKey = p.StoreName;
            p.RowKey = Guid.NewGuid().ToString();

            insertOperation = TableOperation.Insert(p);
            // Execute the insert operation. 
            table.Execute(insertOperation);

            p = new ProductURL();
            p.Category = Category.Cloth.ToString();
            p.StoreName = "Outlets";
            p.ProductUrl = "http://www.premiumoutlets.com/outlets/sales.asp?id=110";
            p.IsActive = true;
            p.PartitionKey = p.StoreName;
            p.RowKey = Guid.NewGuid().ToString();

            insertOperation = TableOperation.Insert(p);
            // Execute the insert operation. 
            table.Execute(insertOperation);

        }
    }
}
