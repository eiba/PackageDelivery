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
    public class HomeController : Controller
    {
        private ApplicationDbContext context = new ApplicationDbContext();

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

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        public ActionResult Quote(string weight, string height, string length, string width, string cost, string speed)
        {
            if (weight != null && height != null && length != null && width != null && cost != null && speed != null)
            {
                return RedirectToAction("Order", new { weight = weight, length = length, height = height, cost = cost, width = width, speed=speed });
            }

            ViewBag.Message = "Get a quote on your delivery before you pay";

            return View();
        }
        [HttpPost]
        public ActionResult Quote(string something)
        {
            
            ViewBag.Message = "Get a quote on your delivery before you pay";

            return View();
        }

        //Shows order request form
        [Authorize(Roles = "Owner,Admin,Customer")]
        public ActionResult Order(string errormessage, string weight, string length, string height, string cost, string width, string speed)
        {
            if (errormessage != null)
            {
                ViewBag.Error = errormessage;
            }
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            ApplicationUser user = manager.FindByIdAsync(User.Identity.GetUserId()).Result;
            var adress = context.Adresses.Find(user.AdressId);
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
                if (speed == "high")
                {
                    priority = Priority.High;

                }else if (speed == "medium")
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

        //Order creation post method
        [Authorize(Roles = "Owner,Admin,Customer")]
        [HttpPost]
        public ActionResult Order(OrderModel model)
        {

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home", new { errormessage = "Something went wrong, sorry" });

            }
            
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            ApplicationUser user = manager.FindByIdAsync(User.Identity.GetUserId()).Result;     //current user

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
                var pickup = adressExist(pickupAdress);     //Checks if adress is already in the database or not
                if (pickup == null)
                {
                    context.Adresses.Add(pickupAdress);
                    context.SaveChanges();
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
                var delivery = adressExist(deliveryAdress);
                if (delivery == null)
                {
                    context.Adresses.Add(deliveryAdress);
                    context.SaveChanges();
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
                context.Orders.Add(order);
                context.SaveChanges();
                double WhichSpeed = 1.0;
                if (model.PackageInfo.Priority == Priority.Low)
                {
                    WhichSpeed = 1.0;
                }
                if (model.PackageInfo.Priority == Priority.Medium)
                {
                    WhichSpeed = 1.5;
                }
                if (model.PackageInfo.Priority == Priority.High)
                {
                    WhichSpeed = 2.0;
                }
                //int employeecount = Request.Form["employees"].AsInt();
                double PriceIncrease = (WhichSpeed - 1.0) * 100.0;
                double VolumePrice = (model.PackageInfo.Height * model.PackageInfo.Length * model.PackageInfo.Width) / 100000000.0;
                double WeightPrice = ((model.PackageInfo.Weight) * 4.0);
                double SpeedPrice = ((VolumePrice + WeightPrice) * WhichSpeed) - (VolumePrice + WeightPrice);
                double TotalPrice = (VolumePrice + WeightPrice) * WhichSpeed;
                var package = new Packages
                {
                    SenderId = user.Id,
                    RecieverName = model.PackageInfo.RecieverName,
                    Weight = model.PackageInfo.Weight,
                    Length = model.PackageInfo.Length,
                    Width = model.PackageInfo.Width,
                    Height = model.PackageInfo.Height,
                    SpecialInstructions = model.PackageInfo.sInstructions,
                    RecieverAdressId = deliveryAdress.AdressId,
                    OrderId = order.OrderId,
                    Cost = TotalPrice        
                };

                context.Packages.Add(package);
                context.SaveChanges();
            
            return RedirectToAction("Index","Home", new {successmessage= "Your order has been recieved, thank you!" });
        }

        //Check if adress is already in database, if it is return that adress instead
        public Adresses adressExist(Adresses adress)
        {
            var Adresses = context.Set<Adresses>();
            foreach (var Adress in Adresses)
            {
                if (adress.StreetAdress == Adress.StreetAdress && adress.PostCode == Adress.PostCode)
                {
                    return Adress;
                }  
            }
            return null;
        }
       
    }
}