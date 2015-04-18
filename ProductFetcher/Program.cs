using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using ProductData;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

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
        // This is not suppose to be run all the time. 
        private static void GenerateProductUrlsTable()
        {
            string conn = ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString;
            IProductUrlStorage productUrlStorage = new ProductUrlStorage(conn);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(conn);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("ProductUrls");
            table.CreateIfNotExists();

            string fileName = Path.Combine(Environment.CurrentDirectory, "ProductUrls.csv");

            String[] values = File.ReadAllText(fileName).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            foreach(string s in values)
            {
                if (!string.IsNullOrEmpty(s.Trim()))
                {
                    string[] columns = s.Split('\t');

                    ProductURL p = new ProductURL();
                    p.StoreChain = columns[0];
                    p.StoreName = columns[1];
                    p.Category = columns[2];
                    p.ProductUrl = columns[3];
                    if (columns.Length >4)
                        p.Zipcode = columns[4]; 
                    p.IsActive = true;

                    productUrlStorage.AddProductUrl(p);
                }
            }
        }
    }
}
