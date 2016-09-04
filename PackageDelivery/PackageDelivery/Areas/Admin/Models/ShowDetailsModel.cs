using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PackageDelivery.Models;
using PackageDelivery.Models;

namespace PackageDelivery.Areas.Admin.Models
{
    public class ShowDetailsModel
    {
        public ApplicationUser user { get; set; }
        public string VueId { get; set; }
        public string modalId { get; set; }
    }
}