using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ProductsAdmin.Models;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;
using PagedList;

namespace ProductsAdmin.Controllers
{
    public class ProductsController : Controller
    {
        // Retrieve the storage account from the connection string.
        public static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.AppSettings["StorageConnectionString"]);
        // Create the table client.
        public static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        CloudTable table = tableClient.GetTableReference("Products");

        private ApplicationDbContext db = new ApplicationDbContext();
        private async Task<Product> FindRowAsync(string partitionKey, string rowKey)
        {
            var retrieveOperation = TableOperation.Retrieve<Product>(partitionKey, rowKey);
            var retrievedResult = await table.ExecuteAsync(retrieveOperation);
            var Product = retrievedResult.Result as Product;
            if (Product == null)
            {
                throw new Exception("No Product found for: " + partitionKey);
            }

            return Product;
        }

        // GET: Products
        public ActionResult Index()
        {
            var results = (from entity in table.CreateQuery<Product>()
                           //where entity.PartitionKey == "奥特莱"
                           select entity).Take(100).ToList();
            return View(results);
        }
        public ActionResult Costco()
        {
            var results = (from entity in table.CreateQuery<Product>()
                           where entity.StoreChain == "costco"
                           select entity).ToList();
            return View(results);
        }
        public ActionResult Macys()
        {
            var results = (from entity in table.CreateQuery<Product>()
                           where entity.StoreChain == "macys"
                           select entity).ToList();
            return View(results);
        }
        public ActionResult Outlets()
        {
            var results = (from entity in table.CreateQuery<Product>()
                           where entity.StoreChain != "macys" && entity.StoreChain != "costco"
                           select entity).ToList();
            return View(results);
        }

        // GET: Products/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductId,ProductName,Store,StoreChain,ProductSKU,ProductURL,ProductImage,ProductDescription,Category,CouponStartDate,CouponEndDate,CouponDetail,OriginalPrice,SalePrice,SaleCity,PartitionKey,RowKey,Timestamp,ETag")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<ActionResult> Edit(string partitionKey, string rowKey)
        {
            var product = await FindRowAsync(partitionKey, rowKey);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string partitionKey, string rowKey, Product editedProduct)
        {
            if (ModelState.IsValid)
            {
                var product = new Product();
                UpdateModel(editedProduct);
                try
                {
                    var replaceOperation = TableOperation.Replace(editedProduct);
                    await table.ExecuteAsync(replaceOperation);
                    return RedirectToAction("Index");
                }
                catch (StorageException ex)
                {
                    if (ex.RequestInformation.HttpStatusCode == 412)
                    {
                        // Concurrency error
                        var retrieveOperation = TableOperation.Retrieve<Product>(partitionKey, rowKey);
                        var retrievedResult = table.Execute(retrieveOperation);
                        var currentProduct = retrievedResult.Result as Product;
                        if (currentProduct == null)
                        {
                            ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                + "was deleted by another user after you got the original value. The "
                                + "edit operation was canceled. Click the Back to List hyperlink.");
                        }
                        if (currentProduct.ProductDescription != editedProduct.ProductDescription)
                        {
                            ModelState.AddModelError("ProductDescription", "Current value: " + currentProduct.ProductDescription);
                        }
                        if (currentProduct.ProductDescription != editedProduct.ProductDescription)
                        {
                            ModelState.AddModelError("Description", "Current value: " + currentProduct.ProductDescription);
                        }
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                            + "was modified by another user after you got the original value. The "
                            + "edit operation was canceled and the current values in the database "
                            + "have been displayed. If you still want to edit this record, click "
                            + "the Save button again. Otherwise click the Back to List hyperlink.");
                        ModelState.SetModelValue("ETag", new ValueProviderResult(currentProduct.ETag, currentProduct.ETag, null));
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(editedProduct);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
