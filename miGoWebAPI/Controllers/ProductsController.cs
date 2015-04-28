using ProductData;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Http;
using System.Web.Http.Description;

namespace miGoWebAPI.Controllers
{
    public class ProductsController : ApiController
    {
        // GET: api/Coupon

        private readonly IProductStorage storage;

        public ProductsController()
            : this(new ProductStorage(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString, "Products"))
        {
        }

        public ProductsController(IProductStorage productStorage)
        {
            if (productStorage == null)
            {
                throw new ArgumentNullException("productStorage");
            }

            this.storage = productStorage;
        }

        [HttpGet]
        [ActionName("all")]
        public IEnumerable<Product> GetProducts()
        {
            return this.storage.GetAllProducts();
        }

        [HttpGet]
        [ActionName("storechain")]
        public IEnumerable<Product> FindProductsByStorechain(string storeChainName)
        {
            return this.storage.GetProductsByStoreChain(storeChainName);
        }

        [HttpGet]
        [ActionName("key")]
        public Product FindProductByKeys(string partitionKey, string rowKey)
        {
            return this.storage.GetProductByKeys(partitionKey, rowKey);
        }

        [HttpGet]
        [ActionName("name")]
        public IEnumerable<Product> GetProductsByName(string productName)
        {
            return this.storage.GetProductsByName(productName);
        }

        //[Route("{category}")]
        [HttpGet]
        [ActionName("category")]
        public IEnumerable<Product> GetProductsByCategory(string category)
        {
            Category c;

            Enum.TryParse<Category>(category, out c);
            return this.storage.GetProductsByCategory(c);
        }

        [HttpGet]
        [ActionName("today")]
        public IEnumerable<Product> FindProductByKeys()
        {
            return this.storage.GetAllProductsToday();
        }
    }
}
