using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductData
{
    public interface IImageStorage
    {
        IEnumerable<string> GetShufflesOlderThan(DateTime date);

        IEnumerable<Uri> GetAllShuffleParts(string shuffleId);

        Uri GetImageLink(string shuffleId);

        bool IsReadonly(string shuffleId);

        void AddNewPart(string shuffleId, string fileName, Stream fileStream);

        void RequestShuffle(string shuffleId);

        void Delete(string shuffleId);
    }

    public interface IProductStorage
    {
        IEnumerable<Product> GetAllProducts();
        IEnumerable<Product> GetAllProductsByStore(string storeName);
        IEnumerable<Product> GetProductsByName(string productName);
        Product GetProductsById(Guid id);
        IEnumerable<Product> GetProductsByCategory(Category category);
        IEnumerable<Product> GetProductsExpired();
        void Delete(IEnumerable<Product> products);
        void AddProduct(Product p);

    }

    public interface IProductUrlStorage
    {
        IEnumerable<ProductURL> GetAllProductUrls(bool active = true);
        IEnumerable<ProductURL> GetAllProductsByStore(string storeName, bool active = true);
    }

    public enum Category
    {
        [Display(Name = "Health Care")]
        Health,
        [Display(Name = "Beauty")]
        Beauty,
        [Display(Name = "Cloth")]
        Cloth
    }

    public class Product : TableEntity
    {
        public Guid ProductId { get; set; }

        [StringLength(500)]
        [DefaultValue("test")]
        public string ProductName { get; set; }

        [StringLength(100)]
        [DefaultValue("test")]
        public string Store { get; set; }


        [StringLength(100)]
        [DefaultValue("test")]
        public string ProductSKU { get; set; }

        [StringLength(8000)]
        [DefaultValue("test")]
        public string ProductURL { get; set; }

        [StringLength(8000)]
        [DefaultValue("test")]
        public string ProductImage { get; set; }
        

        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        [DefaultValue("test")]
        public string ProductDescription { get; set; }
        public string Category { get; set; }

        [DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CouponStartDate { get; set; }


        [DataType(DataType.Date)]
        [DefaultValue("01/10/2001")]
        //[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CouponEndDate { get; set; }

        [StringLength(1000)]
        [DefaultValue("test")]
        [DataType(DataType.MultilineText)]
        public string CouponDetail { get; set; }

        [StringLength(2083)]
        [DisplayName("Full-size Image")]
        [DefaultValue("test")]
        public string CouponImage { get; set; }

        [DefaultValue(0.0)]
        public Double OriginalPrice { get; set; }
        [DefaultValue(0.0)]
        public Double SalePrice { get; set; }

        [StringLength(100)]
        [DefaultValue("test")]
        public string SaleCity { get; set; }
    }

    public class Store
    {
        public Guid StoreId { get; set; }

        [StringLength(100)]
        public string StoreName { get; set; }

        // Zipcode?
        [StringLength(100)]
        public string Zipcode { get; set; }

        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string StoreDescription { get; set; }

        public Boolean? IsActive { get; set; }
    }

    public class ProductURL : TableEntity
    {
        [StringLength(100)]
        public string StoreName { get; set; }

        [StringLength(100)]
        public string Category { get; set; }

        [StringLength(100)]
        public string ProductUrl { get; set; }

        public Boolean? IsActive { get; set; }
    }
}
