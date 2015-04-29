using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Microsoft.WindowsAzure.Storage;

namespace ProductsAdmin.Models
{
    public class Product : TableEntity
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string Store { get; set; }
        public string StoreChain { get; set; }
        public object Zipcode { get; set; }
        public string ProductSKU { get; set; }
        public string ProductURL { get; set; }
        public string ProductImage { get; set; }
        public string ProductDescription { get; set; }
        public string Category { get; set; }
        public DateTime CouponStartDate { get; set; }
        public DateTime CouponEndDate { get; set; }
        public string CouponDetail { get; set; }
        public object CouponImage { get; set; }
        public Double OriginalPrice { get; set; }
        public Double SalePrice { get; set; }
        public string SaleCity { get; set; }
    }

}