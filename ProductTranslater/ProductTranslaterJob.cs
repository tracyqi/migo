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
            chatripEntities2 ce = new chatripEntities2();
            string conn = ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString;
            IProductStorage productStorage_en = new ProductStorage(conn, "productsen");
             IProductStorage productStorage_ch = new ProductStorage(conn, "Products");
            ILogger logger = new Logger();

            var p = (productStorage_en.GetProductByKeys(msg.Split(',')[0], msg.Split(',')[1]));

            product_en p_ch = new product_en();
            p_ch.Category = p.Category;
            p_ch.CouponDetail = p.CouponDetail;// TranslateText(p.CouponDetail);
            p_ch.CouponEndDate = Convert.ToDateTime(p.CouponEndDate);
            p_ch.CouponImage = p.CouponImage;
            p_ch.CouponStartDate = Convert.ToDateTime(p.CouponStartDate);
            p_ch.OriginalPrice = (float)(p.OriginalPrice);
            p_ch.ProductDescription = p.ProductDescription; // TranslateText(p.ProductDescription);
            p_ch.ProductImage = p.ProductImage;
            p_ch.ProductName = p.ProductName;// TranslateText(p.ProductName);
            p_ch.ProductSKU = p.ProductSKU;
            p_ch.ProductURL = p.ProductURL;
            p_ch.SaleCity = p.SaleCity;//TranslateText(p.SaleCity);
            p_ch.SalePrice = (float)p.SalePrice;
            p_ch.Store = p.Store;// TranslateText(p.Store);
            p_ch.StoreChain = p.StoreChain;

            //p.IsActive = true;
            p_ch.ProductHash = Utilities.CalculateMD5Hash(string.Concat(p_ch.Category, p_ch.CouponDetail, p_ch.CouponEndDate, p_ch.CouponStartDate, p_ch.ProductName, p_ch.ProductSKU, p_ch.SalePrice, p.SaleCity, p_ch.StoreChain));

            if (ce.product_en.Find(p_ch.ProductHash) == null)
            {
                ce.product_en.Add(p_ch);
                ce.SaveChanges();
            }
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

        public static string TranslateCategory(string input)
        {
            throw new NotImplementedException();
        }

        public static string TranslateProduct(string input)
        {
            throw new NotImplementedException();
        }

    }
}
