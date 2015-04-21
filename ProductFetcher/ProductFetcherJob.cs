using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Net;
using Microsoft.Azure.WebJobs;
using ProductData;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Text;

namespace ProductFetcher
{
    public class ProductFetcherJob
    {
        private readonly IProductStorage productStorage;
        private readonly IProductUrlStorage productUrlStorage;
        private readonly ILogger logger;

        public ProductFetcherJob()
        {
            string conn = ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString;
            productStorage = new ProductStorage(conn, "productsen");
            productUrlStorage = new ProductUrlStorage(conn);
            logger = new Logger();
        }

        private IEnumerable<ProductURL> GetProductUrls()
        {
            return this.productUrlStorage.GetAllProductUrls();
        }

        public string FetchData()
        {
            IEnumerable<ProductURL> urls = GetProductUrls();
            foreach (ProductURL u in urls)
            {
                switch (u.StoreChain)
                {
                    case "Costco":
                        CostcoFetcher(u);
                        break;
                    case "Macys":
                        MacysFetcher(u);
                        break;
                    case "Premium Outlets":
                        OutletFetcher(u);
                        break;
                    default:
                        logger.Error("Wrong Store name");
                        break;
                }
            }

            return string.Empty;
        }

        private void OutletFetcher(ProductURL productUrl)
        {
            logger.Information("Start Outlet");
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlWeb.Load(productUrl.ProductUrl);

            //tegoryName = htmlDocument.DocumentNode.SelectSingleNode("//h1[@class = 'title mb-10']").InnerText.Trim().Replace("&reg;", "");

            string html = htmlDocument.DocumentNode.OuterHtml;

            var listItems = htmlDocument.DocumentNode.SelectSingleNode("//*[contains(@class,'StoreEvents')]").InnerHtml;

            string splitStr = "<div style='border-top: 1px solid #E2E2E2; margin:8px 0 6px 0;padding:0;position:relative;'></div>";
            string[] colCoupons = listItems.Split(new string[] { splitStr }, StringSplitOptions.None);

            foreach (var x in colCoupons)
            {
                try
                {
                    Product product = new Product();
                    product.StoreChain = productUrl.StoreName.ToLower();
                    product.Zipcode = productUrl.Zipcode;
                    product.Category = productUrl.Category;

                    var coupon = x.Replace("<h4 class='cap mb-10'>", "").Replace("</h4>", "").Replace("<br>", "");
                    product.Store = coupon.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0].ToLower();
                    string couponDate = coupon.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[1];
                    //TODO: better approch to parse start/end date
                    if (couponDate.Contains("-"))
                    {
                        product.CouponStartDate = Convert.ToDateTime(couponDate.Split('-')[0]);
                        string temp = couponDate.Split('-')[1];
                        if (Char.IsNumber(temp.Trim()[0]))
                            product.CouponEndDate = Convert.ToDateTime(couponDate.Split('-')[0].Substring(0, 3) + " " + couponDate.Split('-')[1]);
                        else
                            product.CouponEndDate = Convert.ToDateTime(couponDate.Split('-')[1]);
                    }
                    else
                    {
                        product.CouponStartDate = Convert.ToDateTime(couponDate);
                        product.CouponEndDate = product.CouponStartDate;
                    }
                    var eventTemp = coupon.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                    List<string> eventDescription_li = new List<string>(eventTemp);
                    eventDescription_li.RemoveRange(0, 2);
                    product.CouponDetail = string.Join("\n", eventDescription_li.ToArray());

                    productStorage.AddProduct(product);
                }
                catch (Exception e)
                {
                    logger.Error("failed one:", e.StackTrace);
                    continue;
                }
            }
        }
        private void CostcoFetcher(ProductURL productUrl)
        {
            logger.Information("Start Costco");
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlWeb.Load(productUrl.ProductUrl);

            var products = htmlDocument.DocumentNode.SelectNodes("//*[contains(@class,'product-tile comparable')]");
            foreach (var p in products)
            {
                try
                {

                    Product product = new Product();
                    product.Store = productUrl.StoreName.ToLower();
                    product.StoreChain = productUrl.StoreName.ToLower();
                    product.Zipcode = productUrl.Zipcode;

                    //Category c;
                    //Enum.TryParse<Category>(productUrl.Category, out c);
                    product.Category = productUrl.Category;

                    HtmlNode node = p.SelectSingleNode("div[contains(@class,'product-tile-image-container ')]");

                    product.ProductURL = node.SelectSingleNode(".//a[@href]").Attributes["href"].Value;
                    product.ProductSKU = p.Attributes["itemid"].Value;
                    product.ProductName = node.SelectSingleNode(".//*[contains(@class,'short-desc')]").InnerText;
                    product.ProductImage = node.SelectSingleNode(".//*[contains(@class,'product-image')]").Attributes["src"].Value;

                    HtmlDocument htmlDocument2 = htmlWeb.Load(product.ProductURL);
                    var productDetail = htmlDocument2.DocumentNode.SelectSingleNode("//*[contains(@class,'product-price')]");
                    var productValid = htmlDocument2.DocumentNode.SelectSingleNode("//*[contains(@class,'col2')]");
                    product.ProductDescription = htmlDocument2.DocumentNode.SelectSingleNode("//head/meta[contains(@name, 'description')]").Attributes["content"].Value;

                    var y = productDetail.SelectSingleNode(".//*[contains(@class,'merchandisingText')]");

                    if (y != null)
                    {
                        var validDate = y.InnerText.Split(' ');
                        product.CouponStartDate = Convert.ToDateTime(validDate[5]);
                        product.CouponEndDate = Convert.ToDateTime(validDate[7].Replace(".", string.Empty));
                    }
                    else
                    {
                        product.CouponStartDate = DateTime.MaxValue;
                        product.CouponEndDate = DateTime.MaxValue;
                    }

                    var price = productDetail.SelectSingleNode(".//*[contains(@class,'your-price')]").SelectSingleNode(".//*[contains(@class,'currency')]");
                    if (!string.IsNullOrEmpty(price.InnerText))
                    {
                        product.SalePrice = Convert.ToDouble(price.InnerText.Replace("$", ""));
                    }
                    price = productDetail.SelectSingleNode(".//*[contains(@class,'online-price hide-div')]").SelectSingleNode(".//*[contains(@class,'currency')]");
                    if (!string.IsNullOrEmpty(price.InnerText))
                    {
                        product.OriginalPrice = Convert.ToDouble(price.InnerText.Remove('$'));
                    }
                    else
                    {
                        product.OriginalPrice = product.SalePrice;
                    }


                    productStorage.AddProduct(product);
                }
                catch (Exception e)
                {
                    logger.Error("failed one:", e.StackTrace);
                    continue;
                }
            }
        }

        private void MacysFetcher(ProductURL productUrl)
        {
            logger.Information("Start Macy");
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlWeb.Load(productUrl.ProductUrl);


            var categories = htmlDocument.DocumentNode.
                SelectSingleNode("//ul[@class = 'thumbnails large-block-grid-4' and @id = 'thumbnails']");

            foreach (var p in categories.SelectNodes(".//div[@class = 'innerWrapper']"))
            {
                try
                {
                    Product product = new Product();
                    //p.Category = htmlDocument.DocumentNode.SelectSingleNode("//h1[@class = 'currentCategory']").InnerText;
                    product.Store = productUrl.StoreName.ToLower();
                    product.Category = productUrl.Category;
                    product.StoreChain = productUrl.StoreName.ToLower();
                    product.Zipcode = productUrl.Zipcode;

                    product.ProductURL = "http://www1.macys.com" + p.SelectSingleNode(".//a[@href]").Attributes["href"].Value;

                    //<h1 id="productTitle" class="productTitle" itemprop="name">Clinique Cleansing by Clinique for Skin Type 1/2</h1>

                    HtmlDocument htmlDocument2 = htmlWeb.Load(product.ProductURL);
                    string productjson = htmlDocument2.DocumentNode.SelectSingleNode(".//script[@id = 'pdpMainData']").InnerText;

                    dynamic dynObj = JsonConvert.DeserializeObject(productjson);


                    //{
                    //"initializers": {
                    //"mmlCarouselEnabled": false,
                    //"zTailorFeatureEnabled": true
                    //},
                    //"productDetail": {
                    //"id": "2078520",
                    //"name": "Clinique Cleansing by Clinique for Skin Type 1/2",
                    //"giftCard": false,
                    //"categoryId": "55537",
                    //"custRatings": "",
                    //"parentSku": 2078520,
                    //"inStock": "true",
                    //"regularPrice": "89.5",
                    //"salePrice": "",
                    //"title": "Clinique Cleansing by Clinique for Skin Type 1/2",
                    //"imageUrl": "http://slimages.macys.com/is/image/MCY/products/4/optimized/2694624_fpx.tif",
                    //"isChanel": false,
                    //"isMaster": false,
                    //"ratingPercent": 0,
                    //"showReviews": true,
                    //"showQuestionAnswers": true,
                    //"showOffers": true,
                    //"categoryName": "Beauty - Gifts &amp; Value Sets",
                    //"registryMode": ""
                    //}
                    //}

                    product.ProductName = dynObj.productDetail.name; //htmlDocument2.DocumentNode.SelectSingleNode(".//h1[@class = 'productTitle']").InnerText;
                    product.ProductImage = dynObj.productDetail.imageUrl;//htmlDocument2.DocumentNode.SelectSingleNode(".//img").Attributes["data-src"].Value;
                    product.OriginalPrice = dynObj.productDetail.regularPrice;//htmlDocument2.DocumentNode.SelectSingleNode("//span[(@class = 'giftSetValue')]");
                    product.SalePrice = Convert.ToDouble(dynObj.productDetail.salePrice.ToString().Length == 0 ? 0 : dynObj.productDetail.salePrice);// htmlDocument2.DocumentNode.SelectSingleNode("//span[(@class = 'priceSale')]");
                    product.ProductSKU = dynObj.productDetail.id;

                    HtmlDocument htmlDocument3 = htmlWeb.Load(product.ProductURL);

                    var giftOffers = htmlDocument3.DocumentNode.SelectNodes("//div[(@class = 'giftOfferDetails')]");
                    if (giftOffers != null)
                    {
                        foreach (var x in giftOffers)
                        {
                            product.CouponDetail += x.SelectSingleNode(".//div[(@class = 'giftOfferDescription')]").InnerText + Environment.NewLine;

                        }
                    }

                    product.CouponStartDate = DateTime.MaxValue;
                    product.CouponEndDate = DateTime.MaxValue;

                    productStorage.AddProduct(product);
                }
                catch (Exception e)
                {
                    logger.Error("failed one:", e.StackTrace);
                    continue;
                }
            }
        }
    }
}