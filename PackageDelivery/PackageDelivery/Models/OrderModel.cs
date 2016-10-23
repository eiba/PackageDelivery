using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PackageDelivery.Models
{
    public class OrderModel
    {

        public DeliveryAdress DeliveryAdress { get; set; }
        public PickupAdress PickupAdress { get; set; }
        public PackageInfo PackageInfo { get; set; }
    }

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

    public class PackageInfo
    {

        [Display(Name = "Special instructions")]
        public string sInstructions { get; set; }

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