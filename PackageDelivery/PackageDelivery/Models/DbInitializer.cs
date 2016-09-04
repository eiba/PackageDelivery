using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace PackageDelivery.Models
{
    public class DbInitializer : DropCreateDatabaseAlways<ApplicationDbContext>
    {

        // Add some test data, which is always smart to do.
        protected override void Seed(ApplicationDbContext db)
        {
            // These two managers handle storage in the given db context for us
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            roleManager.Create(new IdentityRole("Admin")); // rights to view admin and ansatt
            roleManager.Create(new IdentityRole("Customer")); // The customer role
            roleManager.Create(new IdentityRole("Owner"));// The owner of the website, in addition to admin rights he can only add employees(admins)


            var OwnerAdress = new Adresses
            {
                Suburb = "Some suburb i don't even know",
                State = "Queensland",
                PostCode = 2342,
                StreetAdress = "Streets 1:12"

            };
            
            db.Adresses.Add(OwnerAdress);
            
            var Owner = new ApplicationUser
            {
                UserName = "Owner@qut.edu.au",
                Email = "Owner@qut.edu.au",
                Fname = "Mr.",
                Lname = "Owner",
                AdressId = OwnerAdress.AdressId,
                AccessLvL = "Owner",
                IsEnabeled = true,
                Phone = "04456456266",
                DoB = "12/12/1212"
            };
            userManager.Create(Owner, "Password1.");
            userManager.AddToRole(Owner.Id, "Owner");

            var EmployeeAdress = new Adresses
            {
                Suburb = "South Bank",
                State = "Queensland",
                PostCode = 3455,
                StreetAdress = "Some street 78"

            };
            db.Adresses.Add(EmployeeAdress);
            var Employee = new ApplicationUser
            {
                UserName = "Employee@qut.edu.au",
                Email = "Employee@qut.edu.au",
                Fname = "Mr.",
                Lname = "Employee",
                AdressId = EmployeeAdress.AdressId,
                AccessLvL = "Admin",
                IsEnabeled = true,
                Phone = "876890043",
                DoB = "13/13/1313"
            };
            userManager.Create(Employee, "Password1.");
            userManager.AddToRole(Employee.Id, "Admin");

            db.SaveChanges();
        }
    }
}