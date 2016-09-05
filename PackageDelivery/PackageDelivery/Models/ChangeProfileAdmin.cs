using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PackageDelivery.Models
{
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
        //[RegularExpression(@"^[0-9]$", ErrorMessage = "Must be numbers from 0 to 9")]
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

        //[StringLength(100, ErrorMessage = "{0} Must be at least {2} characters.", MinimumLength = 6)]
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

        public string vueIdd { get; set; }
        public string modalIdd { get; set; }
        public string check { get; set; }
        public string search { get; set; }
    }
}