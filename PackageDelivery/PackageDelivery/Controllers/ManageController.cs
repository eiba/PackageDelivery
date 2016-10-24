using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using PackageDelivery.Areas.Admin.Models;
using PackageDelivery.Models;

namespace PackageDelivery.Controllers
{
    /// <summary>
    /// Controller for managing user account, consists mostly for framework methods. 
    /// Other methods has been commented.
    /// </summary>
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        ApplicationDbContext _context = new ApplicationDbContext();

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        /// <summary>
        /// Method for getting and displaying the index page of user management
        /// </summary>
        /// <param name="message">MessageId for what message to display, error or success</param>
        /// <returns>Returns the front view for user self management</returns>
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : message == ManageMessageId.ChangeProfileSuccess ? "Your info has been successfully updated"
                : "";

            var userId = User.Identity.GetUserId();
            var model1 = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var currentUser = manager.FindById(User.Identity.GetUserId());
            var adress = _context.Adresses.Find(currentUser.AdressId);
            //Gets the custom values
            var model2 = new ChangeProfileViewModel
            {
                State = adress.State,
                PostCode = adress.PostCode,
                Suburb = adress.Suburb,
                StreetAdress = adress.StreetAdress,
                Phone = currentUser.Phone,
            };
            //Model containing all values to be displayed in the view
            var model = new ChangeProfileIdexViewModel
            {
                ChangeProfileViewModel = model2,
                IndexViewModel = model1

            };
            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }


        /// <summary>
        /// Display page with current user info (adress and phone)
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangeProfileInfo()
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())); //Manager for handeling application users
            var currentUser = manager.FindById(User.Identity.GetUserId());                                              //The current logged in user
            var adress = _context.Adresses.Find(currentUser.AdressId);                                                   //Adress of logged in user
            var model = new ChangeProfileViewModel                                                                      //Send in model to view with adress information
            {   
                State = adress.State,
                PostCode = adress.PostCode,
                Suburb = adress.Suburb,
                StreetAdress = adress.StreetAdress,
                Phone = currentUser.Phone,
            };

            return View(model);
        }

        /// <summary>
        /// Post method that changes the profile info of a user.
        /// </summary>
        /// <param name="model">Model from ChangeProfileInfo view, containing the values 
        /// to be changed in the database</param>
        /// <returns>View with either a successmessage or reload of same view upon error when 
        /// changing values.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeProfileInfo(ChangeProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var store = new UserStore<ApplicationUser>(new ApplicationDbContext());
                var manager = new UserManager<ApplicationUser>(store);
                ApplicationUser user = manager.FindById(User.Identity.GetUserId());
                user.Phone = model.Phone;
                var newAdress = new Adresses
                {
                    StreetAdress = model.StreetAdress,
                    State = model.State,
                    Suburb = model.Suburb,
                    PostCode = model.PostCode
                };
                
                var adress = AdressExist(newAdress);
                if (adress == null)
                {
                    _context.Adresses.Add(newAdress);
                    _context.SaveChanges();
                    user.AdressId = newAdress.AdressId;
                }
                else
                {
                    user.AdressId = adress.AdressId;
                }
                

                IdentityResult result = await manager.UpdateAsync(user);
                store.Context.SaveChanges();

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", new { Message = ManageMessageId.ChangeProfileSuccess });
                }
                AddErrors(result);

            }
            return View(model);

        }


        /// <summary>
        /// Returns and displays all the user's current orders. 
        /// </summary>
        /// <returns>The view, which is a list containing the user's orders</returns>
        public ActionResult MyOrders()
        {

            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            ApplicationUser user = manager.FindByIdAsync(User.Identity.GetUserId()).Result;

               var packages = from s in _context.Packages
                where
                    s.SenderId == user.Id
                select s;
            Dictionary<Packages, Orders> map = new Dictionary<Packages, Orders>();

            var newpackages = packages.ToList();
            foreach (var package in newpackages)
            {
                var order = from s in _context.Orders
                            where 
                            s.OrderId == package.OrderId
                            select s;

                map.Add(package,order.First());
            }
            

            var model = new OrderViewModel
            {
                OrderDictionaryMap = map
            };

            return View(model);
        }
        /// <summary>
        /// A get method that gets and return the partial view which is
        /// the details modal box for orders in the customer's order panel
        /// </summary>
        /// <param name="packageId">Id of the package</param>
        /// <param name="orderId">Id of the order</param>
        /// <returns>The details partial view modal box</returns>
        [HttpGet]
        public ActionResult ShowOrderDetails(int? packageId, int? orderId)
        {
            if (packageId == null || orderId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var package = _context.Packages.Find(packageId);
            var order = _context.Orders.Find(orderId);
            var model = new OrderDetailsViewModel
            {
                Package =package,
                Order =order,
                Pickupadress = _context.Adresses.Find(order.PickupAdressId),
                Deliveradress = _context.Adresses.Find(package.RecieverAdressId)
            };

            return PartialView("_OrderDetialsPartial",model);
        }
        /// <summary>
        /// Show the partial view which is a modal box that allows you the customer
        /// to edit an order
        /// </summary>
        /// <param name="packageId">Id of the package</param>
        /// <param name="orderId">Id of the order</param>
        /// <returns>The edit order partial view</returns>
        [HttpGet]
        public ActionResult ShowOrderEdit(int? packageId, int? orderId)
        {
            if (packageId == null || orderId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var package = _context.Packages.Find(packageId);
            var order = _context.Orders.Find(orderId);
            var model = new OrderDetailsViewModel
            {
                Package = package,
                Order = order,
                Pickupadress = _context.Adresses.Find(order.PickupAdressId),
                Deliveradress = _context.Adresses.Find(package.RecieverAdressId)
            };
            ViewBag.date = ConvertDateTime(order.ReadyForPickupTime);
            return PartialView("_OrderEditPartial", model);
        }
        /// <summary>
        /// Edits the order of the user's package, given that it is not picked up yet
        /// </summary>
        /// <param name="packageId">Id of package</param>
        /// <param name="orderId">Id of order</param>
        /// <param name="model"></param>
        /// <returns>A partial view, which asynchronously update the list with orders</returns>
        public async Task<ActionResult> EditOrder(int? packageId, int? orderId, OrderDetailsViewModel model)
        {

            Dictionary<Packages, Orders> map = new Dictionary<Packages, Orders>();  //A dictionary containing the package and the corresponding order
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            ApplicationUser user = manager.FindByIdAsync(User.Identity.GetUserId()).Result;
            if (!ModelState.IsValid)    //If order is not valid, return partial view with error message "Order where not updated".
            {
                var packages = from s in _context.Packages
                                where
                                    s.SenderId == user.Id
                                select s;
                var newpackages = packages.ToList();
                foreach (var packageToAdd in newpackages)
                {
                    var orders = from s in _context.Orders
                                 where
                                 s.OrderId == packageToAdd.OrderId
                                 select s;

                    map.Add(packageToAdd, orders.First());
                }


                var models = new OrderViewModel
                {
                    OrderDictionaryMap = map
                };
                ViewBag.Id = orderId;
                ViewBag.Error = "Order were not updated";
                return PartialView("_orderPartial", models);
            }

                if (model.Order.ReadyForPickupTime < DateTime.Now || model.Order.OrderStatus >= Status.Recieved)//If ready for pickup time is in the past
                {                                                                                               //or package has already been recieved at the warehouse you cannt change order
                var packages = from s in _context.Packages
                               where
                                   s.SenderId == user.Id
                               select s;
                var newpackages = packages.ToList();
                foreach (var packageToAdd in newpackages)
                {
                    var orders = from s in _context.Orders
                                 where
                                 s.OrderId == packageToAdd.OrderId
                                 select s;

                    map.Add(packageToAdd, orders.First());
                }


                var models = new OrderViewModel
                {
                    OrderDictionaryMap = map
                };
                    ViewBag.Id = orderId;
                    ViewBag.Error = "Order ready for pickup date cannot be in the past, or changed after the order has been recieved";
                    return PartialView("_orderPartial", models);
                }
                var package = _context.Packages.Find(packageId);
                var order = _context.Orders.Find(orderId);

                var pickupAdress = new Adresses
                {
                    State = model.Pickupadress.State,
                    PostCode = model.Pickupadress.PostCode,
                    StreetAdress = model.Pickupadress.StreetAdress,
                    Suburb = model.Pickupadress.Suburb,
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
                    StreetAdress = model.Deliveradress.StreetAdress,
                    PostCode = model.Deliveradress.PostCode,
                    Suburb = model.Deliveradress.Suburb,
                    State = model.Deliveradress.State,

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
                //Validation passed, changing values and saving them in database.
                package.SpecialInstructions= model.Package.SpecialInstructions;
                package.RecieverAdressId = deliveryAdress.AdressId;
                _context.Entry(package).State = EntityState.Modified;
                _context.SaveChanges();

                order.PickupAdressId = pickupAdress.AdressId;
                order.ReadyForPickupTime = model.Order.ReadyForPickupTime;

                //Recalculates the time of delivery based on the new new ready for pickup time.
                if (order.OrderPriority == Priority.Low)
                {
                    order.BeginDeliveryTime = order.ReadyForPickupTime.AddDays(7);
                }
                else if (order.OrderPriority == Priority.Medium)
                {
                    order.BeginDeliveryTime = order.ReadyForPickupTime.AddDays(3);
                }
                else
                {
                    order.BeginDeliveryTime = order.ReadyForPickupTime.AddDays(1);
                }

            _context.Entry(order).State = EntityState.Modified;
                _context.SaveChanges();

           
            var Packages = from s in _context.Packages
                           where
                               s.SenderId == user.Id
                           select s;
            var newPackages = Packages.ToList();
            foreach (var Package in newPackages)
            {
                var orderToAdd = from s in _context.Orders
                            where
                            s.OrderId == Package.OrderId
                            select s;

                map.Add(Package, orderToAdd.First());
            }


            var Model = new OrderViewModel
            {
                OrderDictionaryMap = map
            };
            ViewBag.Id = orderId;
            ViewBag.Success = "Successfully updated order " + model.Order.OrderId;
            return PartialView("_orderPartial",Model);
        }

        
        /// <summary>
        /// Method that converts the datetime variable to a displayable format that can be passed to
        /// the datetimepicker in the view.
        /// </summary>
        /// <param name="datetime">Datetime variable to be converted to displayable format</param>
        /// <returns>String that can be correctly placed into a html5 datepicker to show the correct date and time</returns>
        public string ConvertDateTime(DateTime? datetime)
        {
            var convertedString = "";
            if (datetime.HasValue)
            {
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
            var convInt = "";
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

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
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
        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error,
            ChangeProfileSuccess,
            AddProfileSuccess
        }

#endregion
    }
}