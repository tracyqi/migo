using ProductData;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Http;
using System.Web.Http.Description;

namespace miGoWebAPI.Controllers
{
    [RoutePrefix("api/products")]
    public class ProductsController : ApiController
    {
        // GET: api/Coupon

        private readonly IProductStorage storage;

        public ProductsController()
            : this(new ProductStorage(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString))
        {
        }

        public ProductsController(IProductStorage productStorage)
        {
            if (productStorage == null)
            {
                throw new ArgumentNullException("imageStorage");
            }

            this.storage = productStorage;
        }

        [Route("")]
        public IEnumerable<Product> GetProducts()
        {
            return this.storage.GetAllProducts();
        }

        [Route("{store}")]
        public IEnumerable<Product> GetAllProductsByStore(string storeName)
        {
            return this.storage.GetAllProductsByStore(storeName);
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
            Category c = Category.Beauty;

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
