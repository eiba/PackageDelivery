using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PackageDelivery.Models
{
    /// <summary>
    /// Model for changing user information in the Admin panel,
    /// only the owner has access to this and can change all close to 
    /// all the fields in the user accounts. 
    /// </summary>
    public class ChangeProfileAdmin
    {
        [Required]
        [Display(Name = "First Name")]
        public string Fname { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string Lname { get; set; }

        [Required]
        [Display(Name = "Street Adress")]
        public string StreetAdress { get; set; }

        [Required]
        [Display(Name = "Post code")]
        public int PostCode { get; set; }

        [Required]
        [Display(Name = "Suburb")]
        public string Suburb { get; set; }

        [Required]
        [Display(Name = "State")]
        public string State { get; set; }

        [Required]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email adress")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Brukernavn")]
        [DataType("EmailAddress")]
        public string UserName { get; set; }

        [Display(Name = "Id")]
        public string Id { get; set; }

        [Display(Name = "User role")]
        public string AccessLvL { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Passwords must match!")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Bank Account")]
        public string BankAccount { get; set; }

        [Display(Name = "Car rego")]
        public string CarRego { get; set; }

        [Required]
        [Display(Name = "Date of Birth")]
        public string DoB { get; set; }

        public string VueIdd { get; set; }
        public string ModalIdd { get; set; }
        public string Check { get; set; }
        public string Search { get; set; }
    }
}