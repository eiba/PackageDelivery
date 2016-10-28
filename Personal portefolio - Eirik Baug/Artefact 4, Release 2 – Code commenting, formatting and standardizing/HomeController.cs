using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PackageDelivery.Areas.Admin.Models;
using PackageDelivery.Models;

namespace PackageDelivery.Controllers
{   
    /// <summary>
    /// This controller method contains methods for the different views on the front end of the website.
    /// Includes controllers for
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// The context variable allows us to access the database through the entity framework.
        /// </summary>
        private ApplicationDbContext _context = new ApplicationDbContext();

        /// <summary>
        /// Return front page of the application
        /// </summary>
        /// <param name="successmessage">If there is a successmessage, display it</param>
        /// <param name="errormessage">If there is an errormessage, display it</param>
        /// <returns>The view</returns>
        public ActionResult Index(string successmessage, string errormessage)
        {
            if (errormessage != null)
            {
                ViewBag.Error = errormessage;
            }
            else if (successmessage != null)
            {
                ViewBag.Success = successmessage;
            }
            return View();
        }

        /// <summary>
        /// Gets the about page
        /// </summary>
        /// <returns>The about view</returns>
        public ActionResult About()
        {
            ViewBag.Message = "About On The Spot";

            return View();
        }
        /// <summary>
        /// Gets the contact page
        /// </summary>
        /// <returns>The contact view</returns>
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        /// <summary>
        /// Method that returns the quote view
        /// </summary>
        /// <param name="weight">Weight of package</param>
        /// <param name="height">Height of package</param>
        /// <param name="length">Length of package</param>
        /// <param name="width">Width of package</param>
        /// <param name="cost">Cost of package</param>
        /// <param name="speed">Speed/priority of package</param>
        /// <returns>Returns normal qoute view if one or more of the parameters are null, otherwise
        /// returns the order form with the parameters as filled in values</returns>
        public ActionResult Quote(string weight, string height, string length, string width, string cost, string speed)
        {
            if (weight != null && height != null && length != null && width != null && cost != null && speed != null)
            {
                return RedirectToAction("Order", new { weight, length, height, cost, width, speed });
            }

            ViewBag.Message = "Get a quote on your delivery before you pay";

            return View();
        }
        /// <summary>
        /// Post method that display's the cost calcualtion
        /// </summary>
        /// <param name="something">Does nothing, just need a parameter for post</param>
        /// <returns>View with the calcualted package delivery cost</returns>
        [HttpPost]
        public ActionResult Quote(string something)
        {
            
            ViewBag.Message = "Get a quote on your delivery before you pay";

            return View();
        }

        /// <summary>
        /// Shows the order request form with values from quote and the user's adress. If parameters are
        /// are null just returns the basic form
        /// </summary>
        /// <param name="errormessage">If and error happened when processing the order, returns the view
        /// with that error</param>
        /// <param name="weight">Package weight</param>
        /// <param name="length">Package length</param>
        /// <param name="height">Package height</param>
        /// <param name="cost">Package cost</param>
        /// <param name="width">Package width</param>
        /// <param name="speed">Package speed/priority </param>
        /// <returns>The order request form</returns>
        [Authorize(Roles = "Owner,Admin,Customer")]
        public ActionResult Order(string errormessage, string weight, string length, string height, string cost, string width, string speed)
        {
            if (errormessage != null)
            {
                ViewBag.Error = errormessage;
            }
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            ApplicationUser user = manager.FindByIdAsync(User.Identity.GetUserId()).Result;
            var adress = _context.Adresses.Find(user.AdressId);
            var delAdress = new DeliveryAdress      //gets the logged in users adress
            {
                StreetAdress = adress.StreetAdress,
                PostCode = adress.PostCode,
                Suburb = adress.Suburb,
                State = adress.State
            };
            var info = new PackageInfo
            {
                RecieverName = user.Fname + ' ' + user.Lname,
            };
            if (weight != null && height != null && length != null && width != null && cost != null && speed != null)
            {
                var priority = Priority.Low;
                if (speed == "High")
                {
                    priority = Priority.High;

                }else if (speed == "Medium")
                {
                    priority = Priority.Medium;
                }
                info = new PackageInfo
                {
                    RecieverName = user.Fname + ' ' + user.Lname,
                    Height = height.AsInt(),
                    Weight = weight.AsFloat(),
                    Cost = cost.AsInt(),
                    Length = length.AsInt(),
                    Width = width.AsInt(),
                    Priority = priority
                    
                };
            }
            
            var model = new OrderModel
            {
                DeliveryAdress = delAdress,
                PackageInfo = info
                
            };

            return View(model);
        }


        /// <summary>
        /// Order creation post method for adding orders from the order form
        /// to the database.
        /// </summary>
        /// <param name="model">The order model containing </param>
        /// <returns>The front page with an error message if everything goes according to plan,
        /// otherwise returns the order view with an error message</returns>
        [Authorize(Roles = "Owner,Admin,Customer")]
        [HttpPost]
        public ActionResult Order(OrderModel model)
        {

            if (!ModelState.IsValid)    //Model is not valid, return view with error message
            {
                return RedirectToAction("Order", "Home", new { errormessage = "Something went wrong, sorry" });

            }
            
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            ApplicationUser user = manager.FindByIdAsync(User.Identity.GetUserId()).Result;     //Current user

            //A series of if statments checking the form validation, otherwise return the order view with the corresponding error message.
            if (model.PackageInfo.ReadyForPickupTIme < DateTime.Now)
            {
                return RedirectToAction("Order", new {errormessage ="Ready for pickup time cannot be in the past", height = model.PackageInfo.Height.ToString(),weight=model.PackageInfo.Weight.ToString(),width = model.PackageInfo.Width.ToString(), cost = model.PackageInfo.Cost.ToString(),length = model.PackageInfo.Length.ToString(), speed = model.PackageInfo.Priority.ToString()});
            }
            if (model.PackageInfo.Weight <= 0)
            {
                return RedirectToAction("Order", new { errormessage = "Weight cannot be 0", height = model.PackageInfo.Height.ToString(), weight = model.PackageInfo.Weight.ToString(), width = model.PackageInfo.Width.ToString(), cost = model.PackageInfo.Cost.ToString(), length = model.PackageInfo.Length.ToString(), speed = model.PackageInfo.Priority.ToString() });
            }
            if (model.PackageInfo.Height <= 0)
            {
                return RedirectToAction("Order", new { errormessage = "Height cannot be 0", height = model.PackageInfo.Height.ToString(), weight = model.PackageInfo.Weight.ToString(), width = model.PackageInfo.Width.ToString(), cost = model.PackageInfo.Cost.ToString(), length = model.PackageInfo.Length.ToString(), speed = model.PackageInfo.Priority.ToString() });
            }
            if (model.PackageInfo.Length <= 0)
            {
                return RedirectToAction("Order", new { errormessage = "Length cannot be 0", height = model.PackageInfo.Height.ToString(), weight = model.PackageInfo.Weight.ToString(), width = model.PackageInfo.Width.ToString(), cost = model.PackageInfo.Cost.ToString(), length = model.PackageInfo.Length.ToString(), speed = model.PackageInfo.Priority.ToString() });
            }
            if (model.PackageInfo.Width <= 0)
            {
                return RedirectToAction("Order", new { errormessage = "Width cannot be 0" , height = model.PackageInfo.Height.ToString(), weight = model.PackageInfo.Weight.ToString(), width = model.PackageInfo.Width.ToString(), cost = model.PackageInfo.Cost.ToString(), length = model.PackageInfo.Length.ToString(), speed = model.PackageInfo.Priority.ToString() });
            }
            //Creates the order
            var pickupAdress = new Adresses
            {
                StreetAdress = model.PickupAdress.StreetAdress,
                PostCode = model.PickupAdress.PostCode,
                Suburb = model.PickupAdress.Suburb,
                State = model.PickupAdress.State,


                };
                var pickup = AdressExist(pickupAdress);     //Checks if adress is already in the database or not
                if (pickup == null)
                {
                    _context.Adresses.Add(pickupAdress);
                    _context.SaveChanges();
                }
                else
                {
                    pickupAdress = pickup;
                }
                var deliveryAdress = new Adresses
                {
                    StreetAdress = model.DeliveryAdress.StreetAdress,
                    PostCode = model.DeliveryAdress.PostCode,
                    Suburb = model.DeliveryAdress.Suburb,
                    State = model.DeliveryAdress.State,

                };
                var delivery = AdressExist(deliveryAdress);
                if (delivery == null)
                {
                    _context.Adresses.Add(deliveryAdress);
                    _context.SaveChanges();
                }
                else
                {
                    deliveryAdress = delivery;
                }
                var order = new Orders
                {
                    OrderTime = DateTime.Now,
                    PickupAdressId = pickupAdress.AdressId,
                    OrderPriority = model.PackageInfo.Priority,
                    OrderStatus = Status.Requested,
                    ReadyForPickupTime = model.PackageInfo.ReadyForPickupTIme,
                    PaymentType = model.PackageInfo.PaymentType,
                    BeginDeliveryTime = model.PackageInfo.ReadyForPickupTIme
                };
                //Calculates the amount of days it will take to deliver the package based on when it was picked up
                //and the priority of the package
                if (model.PackageInfo.Priority == Priority.Low)
                {
                    order.BeginDeliveryTime = model.PackageInfo.ReadyForPickupTIme.AddDays(7);
                }
                else if (model.PackageInfo.Priority == Priority.Medium)
                {
                    order.BeginDeliveryTime = model.PackageInfo.ReadyForPickupTIme.AddDays(3);
                }
                else
                {
                    order.BeginDeliveryTime = model.PackageInfo.ReadyForPickupTIme.AddDays(1);
                }
                _context.Orders.Add(order);
                _context.SaveChanges();
                //Calculates the cost pased on priority and package dimensions
                var whichSpeed = 1.0;
                if (model.PackageInfo.Priority == Priority.Low)
                {
                    whichSpeed = 1.0;
                }
                if (model.PackageInfo.Priority == Priority.Medium)
                {
                    whichSpeed = 1.5;
                }
                if (model.PackageInfo.Priority == Priority.High)
                {
                    whichSpeed = 2.0;
                }
                var volumePrice = (model.PackageInfo.Height * model.PackageInfo.Length * model.PackageInfo.Width) / 100000000.0;
                var weightPrice = (((model.PackageInfo.Weight)/1000) * 4.0);
                var totalPrice = (volumePrice + weightPrice) * whichSpeed;
                //Creates the package and adds it to the database
                var package = new Packages
                {
                    SenderId = user.Id,
                    RecieverName = model.PackageInfo.RecieverName,
                    Weight = model.PackageInfo.Weight,
                    Length = model.PackageInfo.Length,
                    Width = model.PackageInfo.Width,
                    Height = model.PackageInfo.Height,
                    SpecialInstructions = model.PackageInfo.SInstructions,
                    RecieverAdressId = deliveryAdress.AdressId,
                    OrderId = order.OrderId,
                    Cost = totalPrice        
                };

                _context.Packages.Add(package);
                _context.SaveChanges();
            //Success, return front page with success message.
            return RedirectToAction("Index","Home", new {successmessage= "Your order has been recieved, thank you!" });
        }

        /// <summary>
        /// Method that checks if an adress already exist in the database.
        /// </summary>
        /// <param name="Adress">The adress to check if it already exist</param>
        /// <returns>Returns the adress or null if the adress does not already exist</returns>
        public Adresses AdressExist(Adresses Adress)
        {
            var adresses = _context.Set<Adresses>();
            foreach (var adress in adresses)
            {
                if (Adress.StreetAdress == adress.StreetAdress && Adress.PostCode == adress.PostCode)
                {
                    return adress;
                }  
            }
            return null;
        }
       
    }
}