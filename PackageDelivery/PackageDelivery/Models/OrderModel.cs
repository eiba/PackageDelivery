using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PackageDelivery.Models
{
    /// <summary>
    /// Model to be sent to the view and recieved when the customer places an order.
    /// </summary>
    public class OrderModel
    {

        public DeliveryAdress DeliveryAdress { get; set; }
        public PickupAdress PickupAdress { get; set; }
        public PackageInfo PackageInfo { get; set; }
    }

    /// <summary>
    /// The adress where package will be delivered
    /// </summary>
    public class DeliveryAdress
    {
        [Required]
        [Display(Name = "Street adress")]
        public string StreetAdress { get; set; }

        [Required]
        [DataType(DataType.PostalCode)]
        [Display(Name = "Postcode")]
        public int PostCode { get; set; }

        [Required]
        [Display(Name = "Suburb")]
        public string Suburb { get; set; }

        [Required]
        [Display(Name = "State")]
        public string State { get; set; }
    }
    /// <summary>
    /// The adress where the package will be picked up
    /// </summary>
    public class PickupAdress
    {
        [Required]
        [Display(Name = "Street adress")]
        public string StreetAdress { get; set; }

        [Required]
        [DataType(DataType.PostalCode)]
        [Display(Name = "Postcode")]
        public int PostCode { get; set; }

        [Required]
        [Display(Name = "Suburb")]
        public string Suburb { get; set; }

        [Required]
        [Display(Name = "State")]
        public string State { get; set; }
    }
    
    /// <summary>
    /// General package info
    /// </summary>
    public class PackageInfo
    {

        [Display(Name = "Special instructions")]
        public string SInstructions { get; set; }

        [Required]
        [Display(Name = "Weight(kg)")]
        public double Weight { get; set; }

        [Required]
        [Display(Name = "Length(mm)")]
        public int Length { get; set; }
        [Required]
        [Display(Name = "Width(mm)")]
        public int Width { get; set; }
        [Required]
        [Display(Name = "Height(mm)")]
        public int Height { get; set; }
        [Required]
        [Display(Name = "Cost")]
        public double Cost { get; set; }

        [Required]
        [Display(Name = "Reciever name")]
        public string RecieverName { get; set; }

        [Required]
        [Display(Name = "Priority")]
        public Priority Priority { get; set; }

        
        [DataType(DataType.DateTime)]
        [Display(Name = "Ready for pickup time")]
        [Required]
        public DateTime ReadyForPickupTIme { get; set; }

        [Required]
        [Display(Name = "Payment type")]
        public PaymentType PaymentType { get; set; }
    }
}