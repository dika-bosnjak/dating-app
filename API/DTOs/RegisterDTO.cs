using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs
{
    //RegisterDTO stores the data that is inserted during the registration process
    public class RegisterDTO
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string KnownAs { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public DateOnly? DateOfBirth { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 3)]
        public string Password { get; set; }
    }
}