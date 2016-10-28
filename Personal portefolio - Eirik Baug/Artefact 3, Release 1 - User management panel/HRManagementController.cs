using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Net;
using static PackageDelivery.Controllers.ManageController;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.AspNet.Identity.Owin;
using System.Web.UI.WebControls.Expressions;
using PackageDelivery.Areas.Admin.Models;
using PackageDelivery.Controllers;
using PackageDelivery.Models;


namespace PackageDelivery.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller methods for handeling the user management panel
    /// </summary>
    [Authorize(Roles = "Owner")]
    public class HrManagementController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();

        // GET: Admin/HR

        /// <summary>
        /// Index view for HRmanagement.
        /// </summary>
        /// <param name="message">Status message if profile changed or there was an illegal operation</param>
        /// <param name="search">Search for user</param>
        /// <param name="cause"></param>
        /// <returns>Return the view with all the corresponding users</returns>
        public ActionResult Index(ManageController.ManageMessageId? message, string search, string cause)
        {


            ViewBag.StatusMessage =                                                                     //sets values of different message types
                message == ManageController.ManageMessageId.ChangeProfileSuccess ? "Profile changed."
                : message == ManageController.ManageMessageId.AddProfileSuccess ? "User added to database"
                : message == ManageController.ManageMessageId.Error ? "Illegal operation" + " " + cause
                : "";

            var results = from s in _context.Users                                                       //Gets all users in database, order by last name
                      orderby s.Lname
                      select s;
            

            if (search != null)                                                                         //Find all users that match the search result
            {

                results = from s in _context.Users
                              where
                              s.Fname.Contains(search) ||
                              s.Lname.Contains(search) ||
                              s.Email.Contains(search) ||
                              s.Phone.Contains(search) ||
                              s.AccessLvL.Contains(search)
                              orderby s.Lname
                              select s;
            }

            var searchModel = new SearchModel { Search = search };
            var model = new SearchUserViewModel { SearchModel = searchModel, ApplicationUsers = results };

            return View(model);
        }


        /// <summary>
        /// post method for search to display the right users
        /// </summary>
        /// <param name="param">Search query</param>
        /// <param name="k">Whether "show only active users" is checked or not</param>
        /// <returns>Partial view with users corresponding to the parameters</returns>
        [HttpPost]
        public ActionResult Index(string param, string k)
        {
            var check = k;
            if (check == null)
            {
                check = "true";
            }
            var results = ResolveUsers(param, check);   //resolve what users to show based on parameters

            var model = new ShowModel
            {
                Userss = results,
                Check = check
            };
            return PartialView("_ShowUsersPartial", model);  //Update the partial view with a new list of users

        }


        /// <summary>
        /// Register user method method from the admin panel. Automaticaly adds the users as admin/employee because it's intended that customers can adde themselves
        /// and the owner adds emplyees
        /// </summary>
        /// <param name="model">Model for the user info</param>
        /// <param name="param">Param for what users to show after the list has been reloaded</param>
        /// <returns>Partial view list with updated users if successfull, errormessage if not</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterEmployee(EmployeeRegisterViewModel model, string param)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            ApplicationUser userr = userManager.FindByIdAsync(User.Identity.GetUserId()).Result;        //gets user
            if (userr == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            }

            var showModel = new ShowModel                                                               //What users to show when view is updated
            {
                Userss = ResolveUsers(param, "true"),
                Check = "true"
            };
            var results = from s in _context.Users
                          where
                              s.Email.Contains(model.Email)
                          select s;

            if (results != null)
            {
                foreach (var r in results)
                {
                    if (r.Email == model.Email)             //If the sent in email is already in the database, return error
                    {
                        if (Request.IsAjaxRequest())
                        {
                            ViewBag.Error = "The email is already in use.";
                            return PartialView("_ShowUsersPartial", showModel);
                        }
                    }
                }

            }

            if (ModelState.IsValid)         //Creates the emplyee
            {
                var adress = new Adresses
                {
                    State = model.State,
                    Suburb = model.Suburb,
                    PostCode = model.PostCode,
                    StreetAdress = model.StreetAdress
                };
                _context.Adresses.Add(adress);
                _context.SaveChanges();

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Fname = model.Fname,
                    Lname = model.Lname,
                    AdressId = adress.AdressId,
                    Phone = model.Phone,
                    DoB = model.DoB,
                    AccessLvL = "Admin",            //Adds to admin, as all employees have admin access.
                    IsEnabeled = true
                };
                var result = await userManager.CreateAsync(user, model.Password);
                var employee = new Employees
                {
                    EmployeeId = user.Id,
                    CarRego = model.CarRego,
                    BankAccount = model.BankAccount
                };
                _context.Employees.Add(employee);
                _context.SaveChanges();
                if (result.Succeeded)
                {
                    
                    userManager.AddToRole(user.Id, "Admin"); //add to admin role
                   
                    var userName = user.Fname + " " + user.Lname;
                    if (Request.IsAjaxRequest())
                    {

                        ViewBag.Success = "The user " + model.Email + " was added to the database.";        //success status message
                        ViewBag.Id = user.Id;
                        return PartialView("_ShowUsersPartial", showModel);                                 //return and update view.
                    }
                    return RedirectToAction("Index", "HrManagement", new { Message = ManageController.ManageMessageId.AddProfileSuccess, User = userName });

                }

                AddErrors(result);

            }
            if (Request.IsAjaxRequest())            //Something went wrong. Don't know exactly what, but probably the password check.
            {
                ViewBag.Error = "Oops, something went wrong, check that the passwords matches the criteria!";
                return PartialView("_ShowUsersPartial", showModel);
            }
            return null;
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        //Edit method for user
        /// <summary>
        /// Edit method for editing user. Edits user async and displays the updated list without any page refresh using ajax
        /// and partial views
        /// </summary>
        /// <param name="model">model, info about what need to be changed</param>
        /// <param name="param">search query for what to display when the list is updated</param>
        /// <param name="check">Tells wether "show only active users" is checjed or not</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ChangeProfileAdmin model, string param, string check)
        {

            var k = "";                                     
            var checkedd = check;
            if (check == null) //is "show only active users on?"
            {
                checkedd = "true";
            }
            var showModel = new ShowModel       //Resolves what users to show after update
            {
                Search = param,
                Userss = ResolveUsers(param, check),
                Check = checkedd

            };
            if (ModelState.IsValid)
            {
                var store = new UserStore<ApplicationUser>(_context);
                var manager = new UserManager<ApplicationUser>(store);
                ApplicationUser appUser = manager.FindByName(model.UserName);   //the current user

                var results = from s in _context.Users
                              where
                                  s.Email.Contains(model.Email)
                              select s;
                if (results != null)
                {
                    foreach (var resultk in results)
                    {
                        if (resultk.Id != appUser.Id)       //Check if the emil belongs to the edited user. If not you cannot use it as it is already in use
                        {
                            if (Request.IsAjaxRequest())
                            {
                                ViewBag.Error = "Email is already in use";
                                ViewBag.Id = appUser.Id;
                                return PartialView("_ShowUsersPartial", showModel);
                            }
                            return RedirectToAction("Index", "HrManagement",
                                new { Message = ManageController.ManageMessageId.Error, cause = "Email is already in use" });       //return error message

                        }
                    }
                }


                ApplicationUser userr = manager.FindByIdAsync(User.Identity.GetUserId()).Result;
                if (userr == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                }

                //so far so good, change the details of the user
                appUser.Fname = model.Fname;
                appUser.Lname = model.Lname;
                appUser.Phone = model.Phone;
                appUser.DoB = model.DoB;
                appUser.Phone = model.Phone;
                appUser.UserName = model.Email;
                appUser.Email = model.Email;

                var adress = _context.Adresses.Find(appUser.AdressId);
                adress.PostCode = model.PostCode;
                adress.Suburb = model.Suburb;
                adress.State = model.State;
                adress.StreetAdress = model.StreetAdress;

                store.Context.Entry(adress).State = System.Data.Entity.EntityState.Modified;
                store.Context.SaveChanges();

                if (model.BankAccount != null && model.CarRego != null)
                {
                    var employee = _context.Employees.Find(appUser.Id);
                    employee.BankAccount = model.BankAccount;
                    employee.CarRego = model.CarRego;

                    store.Context.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                    store.Context.SaveChanges();
                }

                IdentityResult result = await manager.UpdateAsync(appUser); //update the user in the databse
                store.Context.SaveChanges();

                if (result.Succeeded)   //if update succeeds
                {

                    if (model.Password != null)
                    {
                        var provider = new DpapiDataProtectionProvider("PackageDelivery");
                        manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(provider.Create("Passwordresetting"));
                        string resetToken = await manager.GeneratePasswordResetTokenAsync(appUser.Id);  //need a token to change the password
                        var presult = await manager.ResetPasswordAsync(appUser.Id, resetToken, model.Password);
                        if (!presult.Succeeded) //if password change does not succeed
                        {
                            if (Request.IsAjaxRequest())
                            {
                                ViewBag.Error = "The passwords must match the critera!";
                                ViewBag.Id = appUser.Id;
                                return PartialView("_ShowUsersPartial", showModel);
                            }
                            return RedirectToAction("Index", "HrManagement", new { Message = ManageController.ManageMessageId.Error });

                        }
                    }
                    if (Request.IsAjaxRequest())        //it succeeds, show success status message
                    {
                        ViewBag.Success = "The user " + model.Email + " was updated.";
                        ViewBag.Id = model.Id;
                        return PartialView("_ShowUsersPartial", showModel);
                    }
                    return RedirectToAction("Index", "HrManagement", new { Message = ManageController.ManageMessageId.ChangeProfileSuccess });
                }
                AddErrors(result);

            }
            if (Request.IsAjaxRequest())
            {                               //Oops, something went wrong. Don't exactly know what, but probably the password.
                ViewBag.Error = "Oops, something went wrong, remember the passwords must match and meet the criteria!";
                ViewBag.Id = model.Id;
                return PartialView("_ShowUsersPartial", showModel);
            }
            return null;
        }

        /// <summary>
        /// Deactivation method of user, deactivating asynchronously, and updates list of users
        /// </summary>
        /// <example>If "show only active users" is checked, the deactivated user will be removed from the list,
        /// otherwise he will be grayed out to show the deactivation</example>
        /// <param name="id">Id of user</param>
        /// <returns>Updated list of users</returns>
        [HttpGet]
        public async Task<ActionResult> Deactivate(string id)
        {
            var store = new UserStore<ApplicationUser>(_context);
            var manager = new UserManager<ApplicationUser>(store);
            ApplicationUser userr = manager.FindByIdAsync(User.Identity.GetUserId()).Result;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (userr.AccessLvL != "Owner") //If you are not the owner, this operation is illegal. Abort.
            {
                return RedirectToAction("Index", new { Message = ManageController.ManageMessageId.Error });
            }
            ApplicationUser user = _context.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            if (user.AccessLvL != "Owner") {            //If you're trying to deactivate the owner accout it is also illegal.
            user.LockoutEnabled = true;
            user.LockoutEndDateUtc = new DateTime(9999, 12, 30);
            user.IsEnabeled = false;
            }

            await manager.UpdateAsync(user);
            _context.SaveChanges();

            if (Request.IsAjaxRequest())           //Updates view responsively
            {
                var model = new StatusModel
                {
                    Id = id,
                    Status = "Activate",
                    Action = "Activate"
                };

                ViewBag.Success = "The user " + user.Email + " has been deactivated.";
                ViewBag.Id = user.Id;
                return PartialView("_StatusPartial", model);
            }

            return RedirectToAction("Index", "HrManagement");
        }

        
        /// <summary>
        /// Method for activating a deactivated user, activating asynchronously, and updates list of users
        /// </summary>
        /// <example>If "show only active users" is checked, the activated user will again show in the list,
        /// and will no longer be grayed out.</example>
        /// <param name="id">Id of user</param>
        /// <returns>Updated list of users</returns>
        public async Task<ActionResult> Activate(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var store = new UserStore<ApplicationUser>(_context);
            var manager = new UserManager<ApplicationUser>(store);
            ApplicationUser user = _context.Users.Find(id);                                  //Finds user
            ApplicationUser userr = manager.FindByIdAsync(User.Identity.GetUserId()).Result;//Finds logged in user

            if (user == null)
            {
                return HttpNotFound();
            }
            if (userr.AccessLvL != "Owner") //if the current user is not the owner this operation is illegal. Abort.
            {
                return RedirectToAction("Index", new { Message = ManageController.ManageMessageId.Error });
            }
            

            user.LockoutEnabled = false;                //sets the lockout
            user.IsEnabeled = true;

            await manager.UpdateAsync(user);
            _context.SaveChanges();                      //saves the database

            if (Request.IsAjaxRequest())                //changes the variables in the view for display
            {
                var model = new StatusModel
                {
                    Id = id,
                    Status = "Deactivate",
                    Action = "Deactivate"
                };

                ViewBag.Success = "The user " + user.Email + " has been activated.";        //status message that the user has been activated.
                ViewBag.Id = user.Id;
                return PartialView("_StatusPartial", model);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Resolves the users based on the serach criteria - param and k which is if the user has "show only active users" on or not
        /// </summary>
        /// <param name="param">Search query</param>
        /// <param name="k">Tells wether "show only active users is on or not"</param>
        /// <returns>Returns the list of the users corresponding to these criterias</returns>
        public IEnumerable<ApplicationUser> ResolveUsers(string param, string k)
        {
            var results = from s in _context.Users
                          orderby s.Lname
                          select s;


            if (k == "false" && param != null)
            {
                results = from s in _context.Users
                          where
                          s.Fname.Contains(param) ||
                          s.Lname.Contains(param) ||
                          s.Email.Contains(param) ||
                          s.Phone.Contains(param) ||
                          s.AccessLvL.Contains(param)
                          orderby s.Lname
                          select s;
            }

            if (k == "true" && param == null)
            {
                results = from s in _context.Users
                          where
                          s.IsEnabeled.ToString().Contains("true")
                          orderby s.Lname
                          select s;
            }

            if (k == "true" && param != null)
            {
                results = from s in _context.Users
                          where
                          (s.Fname.Contains(param) ||
                          s.Lname.Contains(param) ||
                          s.Email.Contains(param) ||
                          s.Phone.Contains(param) ||
                          s.AccessLvL.Contains(param)) &&
                          s.IsEnabeled.ToString().Contains("true")
                          orderby s.Lname
                          select s;

            }

            return results;
        }

        /// <summary>
        /// Shows the employee create modal, by loading it asynchronously is as a partial view with ajax
        /// </summary>
        /// <param name="param">Search query</param>
        /// <returns>The modal box for creating new employees as a partial view</returns>
        [HttpGet]
        public ActionResult ShowEmployeeCreate(string param)
        {
            var model = new EmployeeRegisterViewModel
            {
                Param = param

            };

            return PartialView("_RegisterEmployeePartial", model);
        }

        //Get method for the show employee details
        /// <summary>
        /// Shows the employee deatails modal, by loading it asynchronously is as a partial view with ajax
        /// </summary>
        /// <param name="vueId">Id of the vue instance, used to create the modal box</param>
        /// <param name="modalId">Id of the vue modal box</param>
        /// <param name="id">Id of the user</param>
        /// <returns>The modal box for showing employee details as a partial view</returns>
        [HttpGet]
        public ActionResult ShowEmployeeDetails(string vueId, string modalId, string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var employee = new Employees();
           
            ApplicationUser user = _context.Users.Find(id);
            var adress = _context.Adresses.Find(user.AdressId);

            if (_context.Employees.Find(id) != null)     //if the user is an  emplyee
            {
                employee = _context.Employees.Find(id);
            }
  
            
            var showDetailsModel = new ShowDetailsModel
            {
                User = user,
                Adress = adress,
                Employee = employee,
                VueId = vueId,
                ModalId = modalId
            };

            return PartialView("_DetailsPartial", showDetailsModel);
        }

        /// <summary>
        /// Gets the modal box for employee editing, by loading it asynchronously is as a partial view with ajax
        /// </summary>
        /// <param name="vueIdd">Id of the vue instance, used to create the modal box</param>
        /// <param name="modalIdd">Id of the vue modal box</param>
        /// <param name="check">Wether "show only active users" is checked or not</param>
        /// <param name="search">Search query in HRmanagement view</param>
        /// <param name="id">User id</param>
        /// <returns>Returns the modal box for editing a user as a partial view, filled in with current values
        /// in the form text boxes.</returns>
        [HttpGet]
        public ActionResult ShowEmployeeEdit(string vueIdd, string modalIdd, string check, string search, string id)
        {
            if (id == null)                                             //Id is null, return bad request
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var employee = new Employees();
            ApplicationUser user = _context.Users.Find(id);              //get the requested user
            var adress = _context.Adresses.Find(user.AdressId);          //get it's adress

            if (_context.Employees.Find(id) != null)         //find the employee table if the user is an employee
            {
                employee = _context.Employees.Find(id); 
            }
            else
            {
                employee.CarRego = null;                    //user is not an employee
                employee.BankAccount = null;
            }
            
            var changeProfileModel = new ChangeProfileAdmin     //everything to be shown
            {
                Fname = user.Fname,
                Lname = user.Lname,
                Suburb = user.Adress.Suburb,
                State = user.Adress.State,
                PostCode = user.Adress.PostCode,
                Phone = user.Phone,
                Email = user.Email,
                UserName = user.Email,
                Id = user.Id,
                AccessLvL = user.AccessLvL,
                StreetAdress = adress.StreetAdress,
                CarRego = employee.CarRego,
                BankAccount = employee.BankAccount,
                DoB = user.DoB,
                VueIdd = vueIdd,
                ModalIdd = modalIdd,
                Check = check,
                Search = search

            };

            return PartialView("_EditPartial", changeProfileModel);
        }
    }
}