using System;
using System.Collections.Generic;
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
        public Orders order { get; set; }
        public Packages package { get; set; }

        public Adresses pickupadress { get; set; }
        public Adresses deliveradress { get; set; }
    }
}