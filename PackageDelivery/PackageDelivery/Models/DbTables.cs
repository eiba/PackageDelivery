using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PackageDelivery.Models
{
    [Table("Adresses")]
    public class Adresses
    {
        [Key]
        //[Required]
        public int AdressId { get; set; }

        //[Required]
        public string Suburb { get; set; }
        //[Required]
        public int PostCode { get; set; }
        //[Required]
        public string State { get; set; }
        //[Required]
        public string StreetAdress { get; set; }

        //[ForeignKey("AdressId")]
        public virtual IList<ApplicationUser> User { get; set; }
        //[ForeignKey("AdressId")]
        public virtual IList<Orders> Orders { get; set; }

    }

    [Table("Employees")]
    public class Employees
    {
        [Key,ForeignKey("User")]
        //[Required]
        public string EmployeeId { get; set; }

        //[Required]
        public string BankAccount { get; set; }
        //[Required]
        public string CarRego { get; set; }

        public virtual ApplicationUser User { get; set; }
    }

    [Table("Packages")]
    public class Packages
    {
        [Key]
        //[Required]
        public int PackageId { get; set; }

        [ForeignKey("User")]
        //[Required]
        public string SenderId { get; set; }
        //[Required]
        public string RecieverName { get; set; }
        //[Required]
        public int Weight { get; set; }
        //[Required]
        public string SpecialInstructions { get; set; }
        //[Required]
        public int RecieverAdressId { get; set; }
        [ForeignKey("Order")]
        //[Required]
        public int OrderId { get; set; }
        //[Required]
        public double Cost { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Orders Order { get; set; }
    }

    [Table("Orders")]
    public class Orders
    {
        [Key]
        //[Required]
        public int OrderId { get; set; }

        //[Required]
        public string SenderId { get; set; }
        //[Required]
        public string OrderTime { get; set; }

        [ForeignKey("Adress")]
        //[Required]
        public int PickupAdressId { get; set; }
        //[Required]
        public string ReadyForPickupTime { get; set; }
        //[Required]
        public string WareHouseArrivalTime { get; set; }
        //[Required]
        public Status OrderStatus { get; set; }
        //[Required]
        public Priority OrderPriority { get; set; }
        //[Required]
        public string WareHouseDepartureTime { get; set; }

        public virtual Adresses Adress { get; set; }
        public virtual IList<Packages> Package { get; set; }
    }

    public enum Status
    {
        Recieved,Underway,Completed
    }
    public enum Priority
    {
        Low, Medium, High
    }
}