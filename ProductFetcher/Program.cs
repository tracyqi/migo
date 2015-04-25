using HtmlAgilityPack;
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
            //GenerateOutletUrls();

            InsertDailyMetrics();

            ProductFetcherJob j = new ProductFetcherJob();
            j.FetchData();

        }

        private static void InsertDailyMetrics()
        {
            string conn = ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString;
            IProductStorage productStorage = new ProductStorage(conn, "productsen");

            //productStorage.FindProducts("costco");
            IEnumerable<Product> products = productStorage.FindTopProducts();

            foreach (var c in products)
            {
                Console.WriteLine(string.Format ("{0}, {1}, {2},{3}, ", c.ProductName, c.Store, c.OriginalPrice, c.SalePrice));
            }
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

        private static void GenerateOutletUrls()
        {
            string url = "http://www.premiumoutlets.com/centers/index.asp";

            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlWeb.Load(url);
            //<select name="selectstate" class="selectmenu mb-20 cap" id="selectstate" >
            //  <option data-title="Phoenix Premium Outlets" data-short="Phoenix" data-address="4976 Premium Outlets Way" data-city="Chandler" data-state="AZ" data-zip="85226" value="105">
            // Premium Outlets	Columbia Gorge Premium Outlets	Cloth	http://www.premiumoutlets.com/outlets/sales.asp?id=28	97060

            foreach (var p in htmlDocument.DocumentNode.SelectNodes(".//option[@data-title]"))
            {
               // var data = p.SelectSingleNode(".//option[@data-title]");
                string datatitle = p.Attributes["data-title"].Value;
                string zip = p.Attributes["data-zip"].Value;
                string id = p.Attributes["value"].Value;

                using (StreamWriter w = File.AppendText(@"f:\ProductUrls.csv"))
                {
                    w.WriteLine(string.Concat("Premium Outlets", "\t", 
                        datatitle, "\t", 
                        "Cloth", "\t", 
                        "http://www.premiumoutlets.com/outlets/sales.asp?id="+id, "\t", 
                        zip                     
                        ));
                }
            }
        }
    }
}
