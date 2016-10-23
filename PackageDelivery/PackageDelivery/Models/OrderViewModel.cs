using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PackageDelivery.Models
{
    public class OrderViewModel
    {
        public Dictionary<Packages,Orders> OrderDictionaryMap { get; set; }
    }

    public class OrderDetailsViewModel
    {
        [Required]
        public Orders order { get; set; }
        [Required]
        public Packages package { get; set; }
        [Required]
        public Adresses pickupadress { get; set; }
        [Required]
        public Adresses deliveradress { get; set; }
    }
}