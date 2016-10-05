using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public ActionResult Orders(string Search)
        {
            if (Search == null)
            {
                return View(context.Packages.ToList());
            }

            
                 var results = from m in context.Packages
                              where
                                  m.OrderId.ToString()==Search ||
                                  m.RecieverName.Contains(Search) ||
                                  m.Cost.ToString()== Search ||
                                  m.SpecialInstructions.Contains(Search) ||
                                  m.Weight.ToString() == Search ||
                                  m.Order.OrderPriority == Search ||
                                  m.Order.OrderStatus == Search ||
                                  m.SenderId == Search ||
                                  m.Order.PaymentType == Search ||
                                  m.Order.ReadyForPickupTime.ToString() == Search ||
                                  m.Order.WareHouseArrivalTime == Search ||
                                  m.Order.WareHouseDepartureTime == Search ||
                                  m.Order.OrderTime.ToString() == Search ||
                                  m.Order.PickupAdressId.ToString() == Search ||
                                  m.RecieverAdressId.ToString() == Search ||
                                  m.PackageId.ToString() == Search
                              orderby m.OrderId
                              select m;
            
            ViewBag.search = Search;

            return View(results);
        }

        public ActionResult OrderDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Packages Package = context.Packages.Find(id);
            if (Package == null)
            {
                return HttpNotFound();
            }

            return View(Package);
        }
    }
}