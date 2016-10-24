using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PackageDelivery.Models
{
    /// <summary>
    /// Order model containsing a dictionary with the packages and their corresponding
    /// orders
    /// </summary>
    public class OrderViewModel
    {
        public Dictionary<Packages,Orders> OrderDictionaryMap { get; set; }
    }

    public class OrderDetailsViewModel
    {
        [Required]
        public Orders Order { get; set; }
        [Required]
        public Packages Package { get; set; }
        [Required]
        public Adresses Pickupadress { get; set; }
        [Required]
        public Adresses Deliveradress { get; set; }
    }
}