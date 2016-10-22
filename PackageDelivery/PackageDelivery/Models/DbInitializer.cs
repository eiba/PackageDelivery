using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
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

            //adding a customer type user to the DB
            var CustomerAddress = new Adresses
            {
                Suburb = "South Bum",
                State = "QueensHat",
                PostCode = 666,
                StreetAdress = "Some street 32"

            };
            db.Adresses.Add(CustomerAddress);
            var Customer = new ApplicationUser
            {
                UserName = "Customer@qut.edu.au",
                Email = "Customer@qut.edu.au",
                Fname = "Mr.",
                Lname = "Customer",
                AdressId = CustomerAddress.AdressId,
                AccessLvL = "Customer",
                IsEnabeled = true,
                Phone = "666666666",
                DoB = "13/13/1366"
            };
            userManager.Create(Customer, "Password1.");
            userManager.AddToRole(Customer.Id, "Customer");

            var order = new Orders
            {
                OrderTime = DateTime.Now,
                PickupAdressId = Owner.AdressId,
                ReadyForPickupTime = new DateTime(2016,12,30,15,40,56),
                BeginDeliveryTime = DateTime.Now,
                OrderStatus = Status.Recieved,
                PaymentType = PaymentType.Credit,
                OrderPriority = Priority.High,
            };
            db.Orders.Add(order);
            var package = new Packages
            {
                SenderId = Owner.Id,
                RecieverName = "Bob Bird",
                Weight = 65.0,
                SpecialInstructions = "Don't let the cat in",
                RecieverAdressId = Employee.AdressId,
                OrderId = order.OrderId,
                Cost = 124.0,
                ReadyForPickupTime = order.ReadyForPickupTime
            };
            db.Packages.Add(package);

            db.SaveChanges();

            var order2 = new Orders
            {
                OrderTime = DateTime.Now,
                PickupAdressId = Owner.AdressId,
                ReadyForPickupTime = DateTime.Now,
                BeginDeliveryTime = DateTime.Now,
                OrderStatus = Status.Recieved,
                PaymentType = PaymentType.Cash,
                OrderPriority = Priority.Low,
            };
            db.Orders.Add(order2);
            var package2 = new Packages
            {
                SenderId = Employee.Id,
                RecieverName = "Jayman",
                Weight = 123.0,
                SpecialInstructions = "I like trains",
                RecieverAdressId = Owner.AdressId,
                OrderId = order2.OrderId,
                Cost = 420.0,
                ReadyForPickupTime = order2.ReadyForPickupTime
            };
            db.Packages.Add(package2);
            
            db.SaveChanges();
            //this method is for when you want to make a database diagram
            /* 
            using (var ctx = new ApplicationDbContext())
            {
                using (var writer = new XmlTextWriter(@"c:\Documents\Model.edmx", Encoding.Default))
                {
                    EdmxWriter.WriteEdmx(ctx, writer);
                }
            }*/
        }
    }
}