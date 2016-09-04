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
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    [Table("ApplicationUser")]
    public class ApplicationUser : IdentityUser
    {
        
        //[Required]
        public string Fname { get; set; }
        //[Required]
        public string Lname { get; set; }
        //[Required]
        [ForeignKey("Adress")]
        public int AdressId { get; set; }
        //[Required]
        public string Phone { get; set; }
        //[Required]
        public string DoB { get; set; }
        public string AccessLvL { get; set; }
        public bool IsEnabeled { get; set; }

        public virtual Adresses Adress { get; set; }
       // [ForeignKey("Id")]
        public virtual IList<Packages> Packages { get; set; }
       //[ForeignKey("Id")]
        public virtual Employees Employee { get; set; }


        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
        public DbSet<Adresses> Adresses { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<Packages> Packages { get; set; }
        public DbSet<Employees> Employees { get; set; }


        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

    }
}