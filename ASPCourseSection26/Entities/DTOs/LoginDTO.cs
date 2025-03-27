using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitiesManager.Core.DTOs
{
    //creating a LoginDTO class to receive and[annotations] validate incoming requests
    public class LoginDTO
    {
        //in a production scenario this validation should be far more extensive and complete, 
        //checking for symbols and spaces, upper/lower normalization, max characters etc
        //validation is kept simple here since the topic of this project is not validation,
        //in production registration validation should be thorough
        [Required(ErrorMessage = "Email cannot be blank")]
        [EmailAddress(ErrorMessage = "Email formatting error")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password cannot be blank")]
        public string Password { get; set; }
    }
}
