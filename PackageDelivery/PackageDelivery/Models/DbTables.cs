using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PackageDelivery.Models
{

    //This class contains all the database tabels except the user table. See the class "IdentityModels.cs" for the ApplicationUser table.
    [Table("Adresses")]
    public class Adresses
    {
        [Key]
        public int AdressId { get; set; }

        public string Suburb { get; set; }
        public int PostCode { get; set; }

        public string State { get; set; }

        public string StreetAdress { get; set; }


        public virtual IList<ApplicationUser> User { get; set; }
        public virtual IList<Orders> Orders { get; set; }


    }

    [Table("Employees")]
    public class Employees
    {
        [Key,ForeignKey("User")]
        public string EmployeeId { get; set; }

        public string BankAccount { get; set; }
    
        public string CarRego { get; set; }

        public virtual ApplicationUser User { get; set; }
    }

    [Table("Packages")]
    public class Packages
    {
        [Key]
        public int PackageId { get; set; }
        [ForeignKey("User")]
        public string SenderId { get; set; }
        public string RecieverName { get; set; }
        public double Weight { get; set; }
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
        //[Required]
        public string ReadyForPickupTime { get; set; }
       
        public string WareHouseArrivalTime { get; set; }
       
        public string OrderStatus { get; set; }
        public string PaymentType { get; set; }
     
        public string OrderPriority { get; set; }

        public string WareHouseDepartureTime { get; set; }

        public virtual Adresses Adress { get; set; }
        //public virtual IList<Packages> Package { get; set; }
    }

    //Sets the different values for priority and status.
    public enum Status
    {
        Recieved,Underway,Completed
    }
    public enum Priority
    {
        Low, Medium, High
    }
}