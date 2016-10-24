using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PackageDelivery.Areas.Admin.Models
{

    /// <summary>
    /// Status model for showing wether a user is activated or deactivated 
    /// </summary>
    public class StatusModel
    {
        public string Status { get; set; }
        public string Id { get; set; }
        public string Action { get; set; }
    }
}