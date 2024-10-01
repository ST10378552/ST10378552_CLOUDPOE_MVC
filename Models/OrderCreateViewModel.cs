using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace ABCRetailApp.Models
{
    public class OrderCreateViewModel
    {
        public string SelectedCustomerId { get; set; }
        public string SelectedProductId { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public DateTime OrderDate { get; set; }
        
        public List<SelectListItem> Customers { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Products { get; set; } = new List<SelectListItem>();
    }
}
