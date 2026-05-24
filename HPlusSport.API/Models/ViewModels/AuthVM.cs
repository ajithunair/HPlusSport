using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HPlusSport.API.Models.ViewModels
{
    public class RegisterVM
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required]
        public string EmailAddress { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class LoginVM
    {
        [Required]
        public string EmailAddress { get; set; }
        
        [Required]
        public string Password { get; set; }
    }

    public class AuthResultVM
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}