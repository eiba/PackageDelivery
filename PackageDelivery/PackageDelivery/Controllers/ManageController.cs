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
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        ApplicationDbContext Context = new ApplicationDbContext();

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

        //
        // GET: /Manage/Index
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
            var adress = Context.Adresses.Find(currentUser.AdressId);
            var model2 = new ChangeProfileViewModel
            {
                State = adress.State,
                PostCode = adress.PostCode,
                Suburb = adress.Suburb,
                StreetAdress = adress.StreetAdress,
                Phone = currentUser.Phone,
                Role = currentUser.AccessLvL, // the variable for the accesslvl being populated
            };
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
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
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

        //Display page with current user info (adress and phone)
        public ActionResult ChangeProfileInfo()
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())); //Manager for handeling application users
            var currentUser = manager.FindById(User.Identity.GetUserId());                                              //The current logged in user
            var adress = Context.Adresses.Find(currentUser.AdressId);                                                   //Adress of logged in user
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeProfileInfo(ChangeProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var store = new UserStore<ApplicationUser>(new ApplicationDbContext());
                var manager = new UserManager<ApplicationUser>(store);
                ApplicationUser Model = manager.FindById(User.Identity.GetUserId());
                Adresses adress = Context.Adresses.Find(Model.AdressId);
                Model.Phone = model.Phone;
                var newAdress = new Adresses
                {
                    StreetAdress = model.StreetAdress,
                    State = model.State,
                    Suburb = model.Suburb,
                    PostCode = model.PostCode
                };
                
                var Adress = adressExist(newAdress);
                if (Adress == null)
                {
                    Context.Adresses.Add(newAdress);
                    Context.SaveChanges();
                    Model.AdressId = newAdress.AdressId;
                }
                else
                {
                    Model.AdressId = Adress.AdressId;
                }
                

                IdentityResult result = await manager.UpdateAsync(Model);
                store.Context.SaveChanges();

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", new { Message = ManageMessageId.ChangeProfileSuccess });
                }
                AddErrors(result);

            }
            return View(model);

        }

        //Returns and displays all the users current orders.
        public ActionResult MyOrders()
        {

            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(Context));
            ApplicationUser user = manager.FindByIdAsync(User.Identity.GetUserId()).Result;

               var packages = from s in Context.Packages
                where
                    s.SenderId == user.Id
                select s;
            Dictionary<Packages, Orders> map = new Dictionary<Packages, Orders>();

            var newpackages = packages.ToList();
            foreach (var package in newpackages)
            {
                var order = from s in Context.Orders
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
        [HttpGet]
        public ActionResult showOrderDetails(int? packageId, int? orderId)
        {
            if (packageId == null || orderId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var package = Context.Packages.Find(packageId);
            var order = Context.Orders.Find(orderId);
            var model = new OrderDetailsViewModel
            {
                package =package,
                order =order,
                pickupadress = Context.Adresses.Find(order.PickupAdressId),
                deliveradress = Context.Adresses.Find(package.RecieverAdressId)
            };

            return PartialView("_OrderDetialsPartial",model);
        }
        [HttpGet]
        public ActionResult showOrderEdit(int? packageId, int? orderId)
        {
            if (packageId == null || orderId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var package = Context.Packages.Find(packageId);
            var order = Context.Orders.Find(orderId);
            var model = new OrderDetailsViewModel
            {
                package = package,
                order = order,
                pickupadress = Context.Adresses.Find(order.PickupAdressId),
                deliveradress = Context.Adresses.Find(package.RecieverAdressId)
            };
            ViewBag.date = convertDateTime(order.ReadyForPickupTime);
            return PartialView("_OrderEditPartial", model);
        }

        public async Task<ActionResult> editOrder(int? packageId, int? orderId, OrderDetailsViewModel model)
        {
            Dictionary<Packages, Orders> map = new Dictionary<Packages, Orders>();
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(Context));
            ApplicationUser user = manager.FindByIdAsync(User.Identity.GetUserId()).Result;
            if (ModelState.IsValid) {

                if (model.order.ReadyForPickupTime < DateTime.Now)
                {
                    var Packagess = from s in Context.Packages
                                   where
                                       s.SenderId == user.Id
                                   select s;
                    var Newpackagess = Packagess.ToList();
                    foreach (var Package in Newpackagess)
                    {
                        var orders = from s in Context.Orders
                                    where
                                    s.OrderId == Package.OrderId
                                    select s;

                        map.Add(Package, orders.First());
                    }


                    var Models = new OrderViewModel
                    {
                        OrderDictionaryMap = map
                    };
                    ViewBag.Id = orderId;
                    ViewBag.Error = "Order ready for pickup date cannot be in the past";
                    return PartialView("_orderPartial", Models);
                }
                var package = Context.Packages.Find(packageId);
            var order = Context.Orders.Find(orderId);

                var pickupAdress = new Adresses
                {
                    State = model.pickupadress.State,
                    PostCode = model.pickupadress.PostCode,
                    StreetAdress = model.pickupadress.StreetAdress,
                    Suburb = model.pickupadress.Suburb,
                };
                var pickup = adressExist(pickupAdress);     //Checks if adress is already in the database or not
                if (pickup == null)
                {
                    Context.Adresses.Add(pickupAdress);
                    Context.SaveChanges();
                }
                else
                {
                    pickupAdress = pickup;
                }
                var deliveryAdress = new Adresses
                {
                    StreetAdress = model.deliveradress.StreetAdress,
                    PostCode = model.deliveradress.PostCode,
                    Suburb = model.deliveradress.Suburb,
                    State = model.deliveradress.State,

                };
                var delivery = adressExist(deliveryAdress);
                if (delivery == null)
                {
                    Context.Adresses.Add(deliveryAdress);
                    Context.SaveChanges();
                }
                else
                {
                    deliveryAdress = delivery;
                }
                package.SpecialInstructions= model.package.SpecialInstructions;
                package.RecieverAdressId = deliveryAdress.AdressId;
                package.ReadyForPickupTime = model.order.ReadyForPickupTime;
                Context.Entry(package).State = EntityState.Modified;
                Context.SaveChanges();

                order.PickupAdressId = pickupAdress.AdressId;
                order.ReadyForPickupTime = model.order.ReadyForPickupTime;
                
                Context.Entry(order).State = EntityState.Modified;
                Context.SaveChanges();

            }

            var Packages = from s in Context.Packages
                           where
                               s.SenderId == user.Id
                           select s;
            var Newpackages = Packages.ToList();
            foreach (var Package in Newpackages)
            {
                var order = from s in Context.Orders
                            where
                            s.OrderId == Package.OrderId
                            select s;

                map.Add(Package, order.First());
            }


            var Model = new OrderViewModel
            {
                OrderDictionaryMap = map
            };
            ViewBag.Id = orderId;
            ViewBag.Success = "Successfully updated order " + model.order.OrderId;
            return PartialView("_orderPartial",Model);
        }

        //Method that converts the datetime to a displayable format in the view
        public string convertDateTime(DateTime? datetime)
        {
            var convertedString = "";
            if (datetime.HasValue)
            {
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

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        //This method checks if the adress sent in already exists in the database. If it doesn't returns null, otherwise returns the adress.
        public Adresses adressExist(Adresses adress)
        {
            var Adresses = Context.Set<Adresses>();
            foreach (var Adress in Adresses)
            {
                if (adress.StreetAdress == Adress.StreetAdress && adress.PostCode == Adress.PostCode)
                {
                    return Adress;
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