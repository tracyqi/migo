﻿using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
        IEnumerable<Product> GetProductsByStore(string storeName);
        IEnumerable<Product> GetProductsByStoreChain(string storeChainName, double salePercentage=0.15);
        IEnumerable<Product> GetProductsByName(string productName);
        Product GetProductsById(Guid id);
        IEnumerable<Product> GetProductsByCategory(Category category);
        IEnumerable<Product> GetProductsExpired();
        void Delete(IEnumerable<Product> products);
        void AddProduct(Product p);
        Product GetProductsByKeys(string primaryKey, string rowKey);
    }

    public interface IProductUrlStorage
    {
        IEnumerable<ProductURL> GetAllProductUrls(bool active = true);
        IEnumerable<ProductURL> GetAllProductsByStore(string storeName, bool active = true);

        void AddProductUrl(ProductURL purl);
    }

    public enum Category
    {
        [Display(Name = "Health Care")]
        Health,
        [Display(Name = "Beauty Prodcut")]
        Cosmetology,
        [Display(Name = "Cloth")]
        Cloth
    }

    public class Product : TableEntity
    {
        public Guid ProductId { get; set; }

        private byte[] productName;
        [StringLength(1000)]
        public string ProductName
        {
            get
            {
                UTF8Encoding utf8 = new UTF8Encoding();
                if (productName == null) return string.Empty;
                else return utf8.GetString(productName);
            }

            set
            {
                UTF8Encoding utf8 = new UTF8Encoding();
                productName = utf8.GetBytes(value);

            }
        }

        [StringLength(100)]
        public string Store { get; set; }

        [StringLength(100)]
        public string StoreChain { get; set; }

        [StringLength(100)]
        public string Zipcode { get; set; }


        [StringLength(100)]
        [DefaultValue("test")]
        public string ProductSKU { get; set; }

        [StringLength(8000)]
        [DefaultValue("test")]
        public string ProductURL { get; set; }

        [StringLength(8000)]
        [DefaultValue("test")]
        public string ProductImage { get; set; }


        private byte[] productDescription;
        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        [DefaultValue("test")]
        public string ProductDescription
        {
            get
            {
                UTF8Encoding utf8 = new UTF8Encoding();
                if (productDescription == null) return string.Empty;
                else return utf8.GetString( productDescription);
            }

            set
            {
                UTF8Encoding utf8 = new UTF8Encoding();
                productDescription = utf8.GetBytes(Regex.Replace(value, @"(&amp;|amp;)", ""));

            }
        }
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
        private byte[] couponDetail;
        [StringLength(1000)]
        public string CouponDetail
        {
            get
            {
                UTF8Encoding utf8 = new UTF8Encoding();
                if (couponDetail == null) return string.Empty;
                else return utf8.GetString(couponDetail);
            }

            set
            {
                UTF8Encoding utf8 = new UTF8Encoding();
                couponDetail = utf8.GetBytes(value);

            }
        }



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

        public string GetRowKey()
        {
            return Utilities.CalculateMD5Hash(string.Concat(
                this.Category,
                this.CouponDetail == null ? string.Empty : this.CouponDetail,
                this.CouponStartDate,
                this.CouponEndDate,
                this.ProductId,
                this.ProductName == null ? string.Empty : this.ProductName,
                //this.ProductSKU, 
                this.OriginalPrice,
                this.SalePrice,
                this.Store
                ));

        }

    }

    public class ProductURL : TableEntity
    {
        [StringLength(100)]
        public string StoreName { get; set; }

        [StringLength(100)]
        public string Category { get; set; }

        [StringLength(100)]
        public string ProductUrl { get; set; }

        [StringLength(100)]
        public string Zipcode { get; set; }

        [StringLength(100)]
        public string StoreChain { get; set; }
        public Boolean? IsActive { get; set; }

        public string GetRowKey()
        {
            return Utilities.CalculateMD5Hash(string.Concat(this.StoreName, this.Category, this.ProductUrl, this.Zipcode));

        }
    }
}
