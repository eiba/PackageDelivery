using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PackageDelivery.Models;

namespace PackageDelivery.Areas.Admin.Models
{
    /// <summary>
    /// Show model to determine what users to show in the admin panel for administering users
    /// </summary>
    public class ShowModel
    {
        public IEnumerable<ApplicationUser> Userss { get; set; }
        public string Check {get; set; }                //Is "show only active users" checked?
        public string Search { get; set; }              //the search
    }
}