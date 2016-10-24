using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace PackageDelivery.Models
{
    /// <summary>
    /// Application user table, for all users in the database, employee does in addition
    /// have an employee table.
    /// </summary>
    [Table("ApplicationUser")]
    public class ApplicationUser : IdentityUser
    {
        
        public string Fname { get; set; }
        public string Lname { get; set; }
        [ForeignKey("Adress")]
        public int AdressId { get; set; }
        public string Phone { get; set; }
        [DataType(DataType.Date)]
        public string DoB { get; set; }
        public string AccessLvL { get; set; }
        public bool IsEnabeled { get; set; }

        public virtual Adresses Adress { get; set; }
        public virtual IList<Packages> Packages { get; set; }

        public virtual Employees Employee { get; set; }

        /// <summary>
        /// Method for asynchronously generating unique and complex ids for the user accounts.
        /// </summary>
        /// <param name="manager">The manager gets and handles everything about the users in the database.</param>
        /// <remarks>Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType</remarks>
        /// <returns>Returns the the id for the user</returns>
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }

    /// <summary>
    /// The database class, containing all the database tables
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
        //The database tables.
        public DbSet<Adresses> Adresses { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<Packages> Packages { get; set; }
        public DbSet<Employees> Employees { get; set; }


        /// <summary>
        /// Creates the database
        /// </summary>
        /// <returns>Returns a new database object</returns>
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

    }
}