using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using HtmlAgilityPack;
using ProductData;
using System.Configuration;

namespace ProductTranslater
{
    public class ProductTranslaterJob
    {
        public ProductTranslaterJob()
        {
        }
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessQueueMessage([QueueTrigger("workerqueue")] string msg, TextWriter log)
        {
            string conn = ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString;
            IProductStorage productStorage_en = new ProductStorage(conn, "productsen");
             IProductStorage productStorage_ch = new ProductStorage(conn, "Products");
            ILogger logger = new Logger();

            var p = (productStorage_en.GetProductsByKeys(msg.Split(',')[0], msg.Split(',')[1]));

            Product p_ch = new Product();
            p_ch.Category = TranslateText(p.Category);
            p_ch.CouponDetail = TranslateText(p.CouponDetail);
            p_ch.CouponEndDate = p.CouponEndDate;
            p_ch.CouponImage = p.CouponImage;
            p_ch.CouponStartDate = p.CouponStartDate;
            p_ch.OriginalPrice = p.OriginalPrice;
            p_ch.PartitionKey = p.PartitionKey;
            p_ch.ProductDescription = TranslateText(p.ProductDescription);
            p_ch.ProductId = p.ProductId;
            p_ch.ProductImage = p.ProductImage;
            p_ch.ProductName = TranslateText(p.ProductName);
            p_ch.ProductSKU = p.ProductSKU;
            p_ch.ProductURL = p.ProductURL;
            p_ch.RowKey = p.RowKey;
            p_ch.SaleCity = TranslateText(p.SaleCity);
            p_ch.SalePrice = p.SalePrice;
            p_ch.Store = TranslateText(p.Store);

            productStorage_ch.AddProduct(p_ch);
        }

        public static string TranslateText(string input)
        {
            string url = String.Format("http://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair=en|zh", input);

            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlWeb.Load(url);

            var result = htmlDocument.DocumentNode.
                SelectSingleNode("//span[@id = 'result_box']");

            return result.InnerText;
        }
    }
}
