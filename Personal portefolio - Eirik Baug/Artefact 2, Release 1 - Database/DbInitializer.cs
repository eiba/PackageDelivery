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
    /// <summary>
    /// The database initializer. Here we seed the database with test data when we run the project.
    /// The database is current DropCreateDatabaseIfModelChanges, so it drops and reseeds the database if any of the models/database tables change
    /// </summary>
    public class DbInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {

        /// <summary>
        /// The seed method for the database, running whenever the database is created.
        /// </summary>
        /// <param name="db"> The database in which we will add values</param>
        protected override void Seed(ApplicationDbContext db)
        {

            // These two managers handle storage in the given db context for us
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            roleManager.Create(new IdentityRole("Admin")); // rights to view admin and ansatt
            roleManager.Create(new IdentityRole("Customer")); // The customer role
            roleManager.Create(new IdentityRole("Owner"));// The owner of the website, in addition to admin rights he can also add employees(admins)

            
            //Adding different test values, like adresses, orders, packages and users
            var ownerAdress = new Adresses
            {
                Suburb = "Some suburb i don't even know",
                State = "Queensland",
                PostCode = 2342,
                StreetAdress = "Streets 1:12"

            };
            
            db.Adresses.Add(ownerAdress);
            
            var owner = new ApplicationUser
            {
                UserName = "Owner@qut.edu.au",
                Email = "Owner@qut.edu.au",
                Fname = "Mr.",
                Lname = "Owner",
                AdressId = ownerAdress.AdressId,
                AccessLvL = "Owner",
                IsEnabeled = true,
                Phone = "04456456266",
                DoB = "12/12/1212"
            };
            userManager.Create(owner, "Password1.");
            userManager.AddToRole(owner.Id, "Owner");

            var employeeAdress = new Adresses
            {
                Suburb = "South Bank",
                State = "Queensland",
                PostCode = 3455,
                StreetAdress = "Some street 78"

            };
            db.Adresses.Add(employeeAdress);
            var employee = new ApplicationUser
            {
                UserName = "Employee@qut.edu.au",
                Email = "Employee@qut.edu.au",
                Fname = "Mr.",
                Lname = "Employee",
                AdressId = employeeAdress.AdressId,
                AccessLvL = "Admin",
                IsEnabeled = true,
                Phone = "876890043",
                DoB = "13/13/1313"
            };
            userManager.Create(employee, "Password1.");
            userManager.AddToRole(employee.Id, "Admin");

            var customerAddress = new Adresses
            {
                Suburb = "South Bum",
                State = "QueensHat",
                PostCode = 666,
                StreetAdress = "Some street 32"

            };
            db.Adresses.Add(customerAddress);
            var customer = new ApplicationUser
            {
                UserName = "Customer@qut.edu.au",
                Email = "Customer@qut.edu.au",
                Fname = "Mr.",
                Lname = "Customer",
                AdressId = customerAddress.AdressId,
                AccessLvL = "Customer",
                IsEnabeled = true,
                Phone = "666666666",
                DoB = "13/13/1366"
            };
            userManager.Create(customer, "Password1.");
            userManager.AddToRole(customer.Id, "Customer");

            var customer2 = new ApplicationUser
            {
                UserName = "eirikbaug@hotmail.com",
                Email = "eirikbaug@hotmail.com",
                Fname = "Eirik",
                Lname = "Baug",
                AdressId = customerAddress.AdressId,
                AccessLvL = "Customer",
                IsEnabeled = true,
                Phone = "666666666",
                DoB = "13/13/1366"
            };
            userManager.Create(customer2, "Password1.");
            userManager.AddToRole(customer2.Id, "Customer");

            var order = new Orders
            {
                OrderTime = DateTime.Now,
                PickupAdressId = owner.AdressId,
                ReadyForPickupTime = DateTime.Now,
                BeginDeliveryTime = new DateTime(2016,10,23,17,45,50),
                OrderStatus = Status.Requested,
                PaymentType = PaymentType.Credit,
                OrderPriority = Priority.High,
            };
            db.Orders.Add(order);
            var package = new Packages
            {
                SenderId = customer2.Id,
                RecieverName = "Bob Bird",
                Weight = 65.0,
                SpecialInstructions = "Don't let the cat in",
                RecieverAdressId = customer2.AdressId,
                OrderId = order.OrderId,
                Cost = 124.0,
            };
            db.Packages.Add(package);

            //Saves the changes added to the database.
            db.SaveChanges();

            var order2 = new Orders
            {
                OrderTime = DateTime.Now,
                PickupAdressId = owner.AdressId,
                ReadyForPickupTime = DateTime.Now,
                BeginDeliveryTime = new DateTime(2016, 10, 24, 17, 45, 50),
                OrderStatus = Status.Requested,
                PaymentType = PaymentType.Cash,
                OrderPriority = Priority.Low,
            };
            db.Orders.Add(order2);
            var package2 = new Packages
            {
                SenderId = employee.Id,
                RecieverName = "Jayman",
                Weight = 123.0,
                SpecialInstructions = "I like trains",
                RecieverAdressId = owner.AdressId,
                OrderId = order2.OrderId,
                Cost = 420.0,
            };
            db.Packages.Add(package2);
            
            db.SaveChanges();

            //This method is for when you want to make a database diagram, of the database.
            //Uncomment and change DropCreateDatabaseIfModelChanges to DropCreateDatabaseAlways
            //to run this method upon database creation.
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