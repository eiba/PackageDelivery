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
using System.Web.WebPages;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PackageDelivery.Models;
using SendGrid;

namespace PackageDelivery.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for all the methods in admin panel except the human recources 
    /// management one
    /// </summary>
    [Authorize(Roles = "Owner, Admin")]
    public class HomeController : Controller
    {
        /// <summary>
        /// Get the database context with this variable
        /// </summary>
        private ApplicationDbContext _context = new ApplicationDbContext();

        // GET: Admin/Home
        /// <summary>
        /// Returns index page of admin
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// Returns the order history page
        /// </summary>
        /// <param name="search">Search query</param>
        /// <param name="from">From date to check from</param>
        /// <param name="to">To date to check to</param>
        /// <returns>The orders corresponding to the filter.
        /// If no filter is specified return every order in the database.</returns>
        public ActionResult Orders(string search, DateTime? @from, DateTime? to)
        {
            
            //If there is a search, but no to and from date
            if(search != "" && (@from == null || to == null)) { 
                 var results = from m in _context.Packages
                              where
                                  m.OrderId.ToString()==search ||
                                  m.RecieverName.Contains(search) ||
                                  m.Cost.ToString()== search ||
                                  m.SpecialInstructions.Contains(search) ||
                                  m.Weight.ToString() == search ||
                                  m.Order.OrderPriority.ToString() == search ||
                                  m.Order.OrderStatus.ToString() == search ||
                                  m.SenderId == search ||
                                  m.Order.PaymentType.ToString() == search ||
                                  m.Order.ReadyForPickupTime.ToString() == search ||
                                  m.Order.OrderTime.ToString() == search ||
                                  m.Order.PickupAdressId.ToString() == search ||
                                  m.RecieverAdressId.ToString() == search ||
                                  m.PackageId.ToString() == search ||
                                  m.Adress.StreetAdress.Contains(search) ||
                                  m.Adress.Suburb.Contains(search) ||
                                  m.Adress.State.Contains(search) ||
                                  m.Adress.PostCode.ToString()== search ||
                                  m.Order.Adress.StreetAdress.Contains(search) ||
                                  m.Order.Adress.State.Contains(search) ||
                                  m.Order.Adress.Suburb.Contains(search) ||
                                  m.Order.Adress.PostCode.ToString() == search
                              orderby m.OrderId
                              select m;
            
            ViewBag.search = search;
            return View(results);
            }
            //If there is a search and to and from date
            if (search != "" && (@from != null && to != null))
            {
                var results = from m in _context.Packages
                    where
                                 (m.Order.ReadyForPickupTime >= @from &&
                                  m.Order.ReadyForPickupTime <= to) &&(
                                  m.OrderId.ToString() == search ||
                                  m.RecieverName.Contains(search) ||
                                  m.Cost.ToString() == search ||
                                  m.SpecialInstructions.Contains(search) ||
                                  m.Weight.ToString() == search ||
                                  m.Order.OrderPriority.ToString() == search ||
                                  m.Order.OrderStatus.ToString() == search ||
                                  m.SenderId == search ||
                                  m.Order.PaymentType.ToString() == search ||
                                  m.Order.ReadyForPickupTime.ToString() == search ||
                                  m.Order.OrderTime.ToString() == search ||
                                  m.Order.PickupAdressId.ToString() == search ||
                                  m.RecieverAdressId.ToString() == search ||
                                  m.PackageId.ToString() == search ||
                                  m.Adress.StreetAdress.Contains(search) ||
                                  m.Adress.Suburb.Contains(search) ||
                                  m.Adress.State.Contains(search) ||
                                  m.Adress.PostCode.ToString() == search ||
                                  m.Order.Adress.StreetAdress.Contains(search) ||
                                  m.Order.Adress.State.Contains(search) ||
                                  m.Order.Adress.Suburb.Contains(search) ||
                                  m.Order.Adress.PostCode.ToString() == search)

                              orderby m.OrderId
                    select m;
                ViewBag.from = ConvertDateTime(@from);
                ViewBag.to = ConvertDateTime(to);
                ViewBag.search = search;
                return View(results);
            }
            //If there is no surch, but there is a to and from date
            if (search == "" && (@from != null && to != null))
            {
                var results = from m in _context.Packages
                              where
                                  m.Order.ReadyForPickupTime >= @from &&
                                  m.Order.ReadyForPickupTime <= to
                              orderby m.OrderId
                              select m;


                ViewBag.from = ConvertDateTime(@from);
                ViewBag.to = ConvertDateTime(to);
                return View(results);
            }
            //Else just return all the packages in the database
            return View(_context.Packages.ToList());
        }
        /// <summary>
        /// Returns page with details about the order and package
        /// </summary>
        /// <param name="id">Order id</param>
        /// <returns>Page with order/package details.</returns>
        public ActionResult OrderDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Packages package = _context.Packages.Find(id);
            if (package == null)
            {
                return HttpNotFound();
            }

            return View(package);
        }
        /// <summary>
        /// Creates sticker for package
        /// </summary>
        /// <param name="id">Id of package</param>
        /// <returns>The sicker for the package</returns>
        public ActionResult StickerMaker(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Packages package = _context.Packages.Find(id);
            if (package == null)
            {
                return HttpNotFound();
            }

            return View(package);
        }
        /// <summary>
        /// Returns page with today's orders to pick up and deliver
        /// </summary>
        /// <param name="errormessage">Errormessage if something went wrong</param>
        /// <param name="successmessage">Successmessage if operation was successfull</param>
        /// <returns>Page for today's pickups and deliveries</returns>
        public ActionResult TodaysOrders(string errormessage,string successmessage)
        {
            if (errormessage != null)
            {
                ViewBag.Error = errormessage;
            }
            else if (successmessage != null)
            {
                ViewBag.Success = successmessage;
            }
            var results = from m in _context.Packages
                where
                    DbFunctions.TruncateTime(m.Order.ReadyForPickupTime) == DateTime.Today
                orderby m.OrderId
                select m;

            return View(results.ToList());
        }
        /// <summary>
        /// Method that converts the datetime variable to a displayable format that can be passed to
        /// the datetimepicker in the view.
        /// </summary>
        /// <param name="datetime">Datetime variable to be converted to displayable format</param>
        /// <returns>String that can be correctly placed into a html5 datepicker to show the correct date and time</returns>
        public string ConvertDateTime(DateTime? datetime)
        {
            var convertedString ="";
            if (datetime.HasValue) { 
            var year = datetime.Value.Year.ToString();
            var month = ConvertInt(datetime.Value.Month);
            var day = ConvertInt(datetime.Value.Day);
            var hour = ConvertInt(datetime.Value.Hour);
            var minute = ConvertInt(datetime.Value.Minute);

            convertedString = year + "-" + month + "-" + day + "T" + hour + ":" + minute;
            }
            return convertedString;
        }
        /// <summary>
        /// Converts the int if it is a single number to have a 0 in front, so that it can be displayed in the datepicker
        /// </summary>
        /// <param name="number">Number to convert</param>
        /// <example>9 is a single number so it returns "09"</example>
        /// <returns>The converted int as a string</returns>
        public string ConvertInt(int number)
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
        /// <summary>
        /// Starts the delivery; changes status to underway and notifies customer by email
        /// </summary>
        /// <param name="id">Id of package</param>
        /// <returns>Returns todays orders view with error or successmessage depending on wether the email
        /// was sent or not</returns>
        public ActionResult StartDelivery(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Packages package = _context.Packages.Find(id);
            if (package == null)
            {
                return HttpNotFound();
            }
            
            package.Order.OrderStatus = Status.Underway;           //Change order status to underway and save it
            _context.Entry(package).State = EntityState.Modified;
            _context.SaveChanges();

            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            ApplicationUser user = manager.FindByIdAsync(package.SenderId).Result;  //Gets the sender of the package
            string to = user.Email; //Sets that user's email as the email to send to
            string from = "noreply@OnTheSpotDelivery.com";  //Dummy email from the website
            MailMessage message = new MailMessage(from, to);    
            message.Subject = "Package is underway!";       //mail message header
            message.Body = "Your package for "+ package.Adress.StreetAdress + " is underway \n Kind regards, On the spot delivery"; //mail message body
            SmtpClient client = new SmtpClient("smtp.sendgrid.net", Convert.ToInt32(587));

            client.UseDefaultCredentials = false;
            var credentials = new NetworkCredential("azure_c2e053642715e025ba3d377408e8c9b2@azure.com", "Password1.");      //credietials to authenticate with sendgrid
            client.Credentials = credentials; 

            try
            {
                client.Send(message);   //Tries to send the message
            }
            catch (Exception ex)
            {
                return RedirectToAction("TodaysOrders", new { errormessage = "The email could not be sent" });  //Email could not be sent, return view with errormessage

            }

            return RedirectToAction("TodaysOrders", new { successmessage = "The delivery was successfully started" });  //successfully sent, return view with success message
        }

        /// <summary>
        /// Starts the pickup by changing status to pickup and sending an email notifying customer
        /// </summary>
        /// <param name="id">Package id</param>
        /// <returns>Returns todays orders view with error or successmessage depending on wether the email
        /// was sent or not</returns>
        public ActionResult StartPickup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Packages package = _context.Packages.Find(id);
            if (package == null)
            {
                return HttpNotFound();
            }

            package.Order.OrderStatus = Status.Pickup;           //Change order status to pickup and save it
            _context.Entry(package).State = EntityState.Modified;
            _context.SaveChanges();

            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            ApplicationUser user = manager.FindByIdAsync(package.SenderId).Result;  //Gets the sender of the package
            string to = user.Email;     //Sender's email
            string from = "noreply@OnTheSpotDelivery.com";  //dummy email from the application
            MailMessage message = new MailMessage(from, to);
            message.Subject = "Pickup is underway!";        //email header
            message.Body = "A driver picking up you package at " + package.Order.Adress.StreetAdress + " is underway \n Kind regards, On the spot delivery";    //email body
            SmtpClient client = new SmtpClient("smtp.sendgrid.net", Convert.ToInt32(587));      //sendgrid client

            client.UseDefaultCredentials = false;
            var credentials = new NetworkCredential("azure_c2e053642715e025ba3d377408e8c9b2@azure.com", "Password1.");  //set credidentials to authenticate with sendgrid
            client.Credentials = credentials;

            try
            {
                client.Send(message);   //try to send message
            }
            catch (Exception ex)
            {
                return RedirectToAction("TodaysOrders", new { errormessage = "The email could not be sent" }); //Email could not be sent, return view with errormessage

            }

            return RedirectToAction("TodaysOrders", new { successmessage = "The pickup was successfully started" }); //Email could was sent, return view with successmessage
        }

        /// <summary>
        /// Completes the delivery by setting status to delivered
        /// </summary>
        /// <param name="id">Id of package</param>
        /// <returns>Returns view with delivery completed successmessage</returns>
        public ActionResult CompleteDelivery(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Packages package = _context.Packages.Find(id);
            if (package == null)
            {
                return HttpNotFound();
            }

            package.Order.OrderStatus = Status.Completed;
            _context.Entry(package).State = EntityState.Modified;
            _context.SaveChanges();


            return RedirectToAction("TodaysOrders", new { successmessage = "The delivery was successfully completed" });
        }

        /// <summary>
        /// Completes the pickup by setting status to delivered
        /// </summary>
        /// <param name="id">Id of package</param>
        /// <returns>Returns view with pickup completed successmessage</returns>
        public ActionResult CompletePickup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Packages package = _context.Packages.Find(id);
            if (package == null)
            {
                return HttpNotFound();
            }

            package.Order.OrderStatus = Status.Recieved;
            _context.Entry(package).State = EntityState.Modified;
            _context.SaveChanges();


            return RedirectToAction("TodaysOrders", new { successmessage = "Pickup completed" });
        }
        /// <summary>
        /// Gets the delay order page allowing the delivery driver to put in how long the order has been delayed
        /// </summary>
        /// <param name="id">Package id</param>
        /// <param name="type">Type of delay, wether it is pickup or delivery delay</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DelayOrder(int? id, string type)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Packages package = _context.Packages.Find(id);
            if (package == null)
            {
                return HttpNotFound();
            }
            ViewBag.Type = type;
            ViewBag.PackageId = package.PackageId;
            return View();
        }
        /// <summary>
        /// Delays the order with the message sent in
        /// </summary>
        /// <param name="packageId">Package id</param>
        /// <param name="message">Message to be sent with the email</param>
        /// <param name="type">"pickup" for pickup delay, otherwise it's a delivery delay</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DelayOrder(string packageId, string message, string type)
        {
            
            if (packageId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var id = packageId.AsInt();
            Packages package = _context.Packages.Find(id);
            if (package == null)
            {
                return HttpNotFound();
            }

            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            ApplicationUser user = manager.FindByIdAsync(package.SenderId).Result;          //sender 
            string to = user.Email;
            string from = "noreply@OnTheSpotDelivery.com";
            MailMessage mailMessage = new MailMessage(from, to);
            mailMessage.Subject = "Package delayed";
            if (type == "pickup")
            {
                mailMessage.Body = "Your order being delivered to " + package.Adress.StreetAdress +     //mail message if there is a pickup delay
                                   " has been delayed by " + message +
                                   ". \n Sorry for the inconvenience, On the spot delivery";

            }
            else
            {
                mailMessage.Body = "Your order being picked up at " + package.Order.Adress.StreetAdress +   //otherwise delivery delay
                                   " has been delayed by " + message +
                                   ". \n Sorry for the inconvenience, On the spot delivery";
            }
            SmtpClient client = new SmtpClient("smtp.sendgrid.net", Convert.ToInt32(587));

            client.UseDefaultCredentials = false;
            var credentials = new NetworkCredential("azure_c2e053642715e025ba3d377408e8c9b2@azure.com", "Password1.");  //setting credidentials
            client.Credentials = credentials;

            try
            {
                client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                return RedirectToAction("TodaysOrders",new {errormessage = "The email could not be sent"}); //Email could not be sent, return view with errormessage
            }


            return RedirectToAction("TodaysOrders", new { successmessage = "The order was successfully delayed" }); //Email was successfully sent, return view with successmessage
        }
    }
}