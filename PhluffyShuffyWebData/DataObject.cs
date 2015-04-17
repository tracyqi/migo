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

    }

    public enum Category
    {
        Health,
        [Display(Name = "Health Care")]
        Beauty,
        [Display(Name = "Beauty")]
        Electronics
    }

    public class Product : TableEntity
    {
        public Guid ProductId { get; set; }

        [StringLength(100)]
        public string ProductName { get; set; }
        
        [StringLength(100)]
        public string ProductSKU { get; set; }
        
        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string ProductDescription { get; set; }
        public Category? Category { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CouponStartDate { get; set; }


        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CouponEndDate { get; set; }

        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string CouponDetail { get; set; }

        [StringLength(2083)]
        [DisplayName("Full-size Image")]
        public string CouponImage { get; set; }

        public Double OriginalPrice { get; set; }
        public Double SalePrice { get; set; }

        [StringLength(100)]
        public string SaleCity { get; set; }
    }

    public class Store
    {
        public Guid StoreId { get; set; }

        [StringLength(100)]
        public string StoreName { get; set; }

        [StringLength(100)]
        public string ProductSKU { get; set; }

        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string StoreDescription { get; set; }

        public Boolean? IsActive { get; set; }
    }
}
