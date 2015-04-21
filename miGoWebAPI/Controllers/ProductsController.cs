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

        public IEnumerable<Product> GetProductsByName(string productName)
        {
            return this.storage.GetProductsByName(productName);
        }

        // GET api/Products/5
        [Route("{id:Guid}")]
        public Product GetProduct(Guid id)
        {
            return this.storage.GetProductsById(id);
        }

        [Route("{category}")]
        public IEnumerable<Product> GetProductsByCategory(string category)
        {
            Category c;

            Enum.TryParse<Category>(category, out c);
            return this.storage.GetProductsByCategory(c);

        }
    }


    //// POST: api/Coupon
    //public void Post([FromBody]string value)
    //{
    //}

    //// PUT: api/Coupon/5
    //public void Put(int id, [FromBody]string value)
    //{
    //}

    //// DELETE: api/Coupon/5
    //public void Delete(int id)
    //{
    //}
    //}
}
