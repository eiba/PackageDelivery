using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls.Expressions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PackageDelivery.Models;
using SendGrid;

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

        public ActionResult Orders(string Search, DateTime? From, DateTime? To)
        {
            /*
            if (Search == null)
            {
                return View(context.Packages.ToList());
            }*/
            
            if(Search != "" && (From == null || To == null)) { 
                 var results = from m in context.Packages
                              where
                                  m.OrderId.ToString()==Search ||
                                  m.RecieverName.Contains(Search) ||
                                  m.Cost.ToString()== Search ||
                                  m.SpecialInstructions.Contains(Search) ||
                                  m.Weight.ToString() == Search ||
                                  m.Order.OrderPriority.ToString() == Search ||
                                  m.Order.OrderStatus.ToString() == Search ||
                                  m.SenderId == Search ||
                                  m.Order.PaymentType.ToString() == Search ||
                                  m.Order.ReadyForPickupTime.ToString() == Search ||
                                  m.Order.WareHouseArrivalTime == Search ||
                                  m.Order.WareHouseDepartureTime == Search ||
                                  m.Order.OrderTime.ToString() == Search ||
                                  m.Order.PickupAdressId.ToString() == Search ||
                                  m.RecieverAdressId.ToString() == Search ||
                                  m.PackageId.ToString() == Search ||
                                  m.Adress.StreetAdress.Contains(Search) ||
                                  m.Adress.Suburb.Contains(Search) ||
                                  m.Adress.State.Contains(Search) ||
                                  m.Adress.PostCode.ToString()== Search ||
                                  m.Order.Adress.StreetAdress.Contains(Search) ||
                                  m.Order.Adress.State.Contains(Search) ||
                                  m.Order.Adress.Suburb.Contains(Search) ||
                                  m.Order.Adress.PostCode.ToString() == Search
                              orderby m.OrderId
                              select m;
            
            ViewBag.search = Search;
            return View(results);
            }
            if (Search != "" && (From != null && To != null))
            {
                var results = from m in context.Packages
                    where
                                 (m.Order.ReadyForPickupTime >= From &&
                                  m.Order.ReadyForPickupTime <= To) &&(
                                  m.OrderId.ToString() == Search ||
                                  m.RecieverName.Contains(Search) ||
                                  m.Cost.ToString() == Search ||
                                  m.SpecialInstructions.Contains(Search) ||
                                  m.Weight.ToString() == Search ||
                                  m.Order.OrderPriority.ToString() == Search ||
                                  m.Order.OrderStatus.ToString() == Search ||
                                  m.SenderId == Search ||
                                  m.Order.PaymentType.ToString() == Search ||
                                  m.Order.ReadyForPickupTime.ToString() == Search ||
                                  m.Order.WareHouseArrivalTime == Search ||
                                  m.Order.WareHouseDepartureTime == Search ||
                                  m.Order.OrderTime.ToString() == Search ||
                                  m.Order.PickupAdressId.ToString() == Search ||
                                  m.RecieverAdressId.ToString() == Search ||
                                  m.PackageId.ToString() == Search ||
                                  m.Adress.StreetAdress.Contains(Search) ||
                                  m.Adress.Suburb.Contains(Search) ||
                                  m.Adress.State.Contains(Search) ||
                                  m.Adress.PostCode.ToString() == Search ||
                                  m.Order.Adress.StreetAdress.Contains(Search) ||
                                  m.Order.Adress.State.Contains(Search) ||
                                  m.Order.Adress.Suburb.Contains(Search) ||
                                  m.Order.Adress.PostCode.ToString() == Search)

                              orderby m.OrderId
                    select m;
                ViewBag.from = convertDateTime(From);
                ViewBag.to = convertDateTime(To);
                ViewBag.search = Search;
                return View(results);
            }
            if (Search == "" && (From != null && To != null))
            {
                var results = from m in context.Packages
                              where
                                  m.Order.ReadyForPickupTime >= From &&
                                  m.Order.ReadyForPickupTime <= To
                              orderby m.OrderId
                              select m;


                ViewBag.from = convertDateTime(From);
                ViewBag.to = convertDateTime(To);
                return View(results);
            }
            return View(context.Packages.ToList());
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

        public ActionResult StickerMaker(int? id)
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

        public ActionResult TodaysOrders()
        {
            var results = from m in context.Packages
                where
                    DbFunctions.TruncateTime(m.Order.ReadyForPickupTime) == DateTime.Today
                orderby m.OrderId
                select m;

            return View(results);
        }

        public string convertDateTime(DateTime? datetime)
        {
            var convertedString ="";
            if (datetime.HasValue) { 
            var year = datetime.Value.Year.ToString();
            var month = convertInt(datetime.Value.Month);
            var day = convertInt(datetime.Value.Day);
            var hour = convertInt(datetime.Value.Hour);
            var minute = convertInt(datetime.Value.Minute);

            convertedString = year + "-" + month + "-" + day + "T" + hour + ":" + minute;
            }
            return convertedString;
        }

        public string convertInt(int number)
        {
            var convInt ="";
            if (number < 10)
            {
                convInt = "0" + number;
            }
            else
            {
                convInt = number.ToString();
            }
            return convInt;
        }

        public ActionResult StartOrder(int? id)
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

            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            ApplicationUser User = manager.FindByIdAsync(Package.SenderId).Result;
            string to = User.Email;
            string from = "noreply@OnTheSpotDelivery.com";
            MailMessage message = new MailMessage(from, to);
            message.Subject = "Package is underway!";
            message.Body = "Your order in underway \n Kind regards, On the spot delivery";
            SmtpClient client = new SmtpClient("smtp.sendgrid.net", Convert.ToInt32(587));

            // Credentials are necessary if the server requires the client 
            // to authenticate before it will send e-mail on the client's behalf.
            client.UseDefaultCredentials = false;
            var credentials = new NetworkCredential("azure_c2e053642715e025ba3d377408e8c9b2@azure.com", "Password1.");
            client.Credentials = credentials; 

            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in CreateTestMessage2(): {0}",
                            ex.ToString());
            }
            
            return RedirectToAction("Index");
        }
    }
}