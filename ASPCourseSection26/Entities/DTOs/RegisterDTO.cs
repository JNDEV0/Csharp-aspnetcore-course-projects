using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitiesManager.Core.DTOs
{
    public class RegisterDTO
    {
        //in a production scenario this validation should be far more extensive and complete, 
        //checking for symbols and spaces, upper/lower normalization, max characters etc
        //validation is kept simple here since the topic of this project is not validation,
        //in production registration validation should be thorough
        [Required(ErrorMessage = "Name cannot be blank")]
        public string Name { get; set; }

        //this Remote tag will send the target property Email to the named actionMethod of givenController, that should return true or false to pass/fail the validation, in this example the method checks if incoming registerDTO request Email is already in the database, there are Identity package options configured @ builder.Services @ Program.cs to handle this unique validation of the username, but this method is used in this example
        //the Remote tag comes from Microsoft.AspNetCore.Mvc.ViewFeatures package
        [Required(ErrorMessage = "Email cannot be blank")]
        [EmailAddress(ErrorMessage = "Email formatting error")]
        [Remote(action: "IsEmailUnique", controller: "account", ErrorMessage = "Email Already in use")]
        public string Email { get; set; }

        [RegularExpression("^[0-9]*$", ErrorMessage = "password cannot be blank")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password cannot be blank")]
        public string Password { get; set; }

        [Required(ErrorMessage = "ConfirmPassword cannot be blank")]
        [Compare("Password", ErrorMessage = "Password and ConfirmPassword must match")]
        public string ConfirmPassword { get; set; }
    }
}
