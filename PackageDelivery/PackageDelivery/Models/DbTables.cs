using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PackageDelivery.Models
{

    // This class contains all the database tabels except the user table. See the class "IdentityModels.cs" for the ApplicationUser table.

    /// <summary>
    /// Database table for adresses.
    /// </summary>
    [Table("Adresses")]
    public class Adresses
    {
        [Key]
        public int AdressId { get; set; }

        [Required]
        public string Suburb { get; set; }
        [Required]
        [Display(Name = "Postcode")]
        public int PostCode { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        [Display(Name = "Street Adress")]
        public string StreetAdress { get; set; }


        public virtual IList<ApplicationUser> User { get; set; }
        public virtual IList<Orders> Orders { get; set; }


    }
    /// <summary>
    /// Database tables for employees
    /// </summary>
    [Table("Employees")]
    public class Employees
    {
        [Key,ForeignKey("User")]
        public string EmployeeId { get; set; }

        public string BankAccount { get; set; }
    
        public string CarRego { get; set; }

        public virtual ApplicationUser User { get; set; }
    }

    /// <summary>
    /// Database tables for packages.
    /// </summary>
    [Table("Packages")]
    public class Packages
    {
        [Key]
        public int PackageId { get; set; }
        [ForeignKey("User")]
        public string SenderId { get; set; }
        public string RecieverName { get; set; }

        [Display(Name = "Weight(gram)")]
        public double Weight { get; set; }
        [Display(Name = "Length(mm)")]
        public int Length { get; set; }
        [Display(Name = "Width(mm)")]
        public int Width { get; set; }
        [Display(Name = "Height(mm)")]
        public int Height { get; set; }
        [Display(Name = "Special instructions")]
        public string SpecialInstructions { get; set; }
        [ForeignKey("Adress")]
        public int? RecieverAdressId { get; set; }
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public double Cost { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual Orders Order { get; set; }
        public virtual Adresses Adress { get; set; }

    }

    /// <summary>
    /// Database tables for orders
    /// </summary>
    [Table("Orders")]
    public class Orders
    {
        [Key]
        public int OrderId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime OrderTime { get; set; }

        [ForeignKey("Adress")]
        public int PickupAdressId { get; set; }

        [DataType(DataType.DateTime)]
        [Required]
        [Display(Name = "Ready for pickup time")]
        public DateTime ReadyForPickupTime { get; set; }
       
        public DateTime BeginDeliveryTime { get; set; }

        public Status OrderStatus { get; set; }
        public PaymentType PaymentType { get; set; }
        public Priority OrderPriority { get; set; }

        public virtual Adresses Adress { get; set; }

    }

    /// <summary>
    /// Status enum, corresponding to the different statues a packages has as it
    /// traverses through the system.
    /// </summary>
    public enum Status
    {
        Requested,Pickup,Recieved,Underway,Completed
    }
    /// <summary>
    /// Package priority, used for cost calculation and delivery speed
    /// </summary>
    public enum Priority
    {
        Low, Medium, High
    }
    /// <summary>
    /// Payment type, corresponding to the payment method chosen by the customer.
    /// </summary>
    public enum PaymentType
    {
        Cash, Credit
    }
}