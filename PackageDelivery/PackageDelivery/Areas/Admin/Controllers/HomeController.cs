using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PackageDelivery.Models;

namespace PackageDelivery.Areas.Admin.Controllers
{
    [Authorize(Roles = "Owner, Admin")]
    public class HomeController : Controller
    {
        private ApplicationDbContext context = new ApplicationDbContext();

        // GET: Admin/Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Orders()
        {

            return View(context.Orders.ToList());
        }
    }
}