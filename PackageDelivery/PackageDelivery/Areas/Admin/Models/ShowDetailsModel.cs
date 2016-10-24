using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PackageDelivery.Models;
using PackageDelivery.Models;

namespace PackageDelivery.Areas.Admin.Models
{   /// <summary>
    /// Model for showing details about employee
    /// </summary>
    public class ShowDetailsModel
    {
        public ApplicationUser User { get; set; }
        public Adresses Adress { get; set; }
        public Employees Employee { get; set; }

        //Ids for the vue modal box
        public string VueId { get; set; }
        public string ModalId { get; set; }
    }
}