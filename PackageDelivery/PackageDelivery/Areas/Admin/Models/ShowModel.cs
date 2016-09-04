using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PackageDelivery.Models;

namespace PackageDelivery.Areas.Admin.Models
{
    public class ShowModel
    {
        public IEnumerable<ApplicationUser> userss { get; set; }
        public string check {get; set; } 
        public string search { get; set; }
    }
}