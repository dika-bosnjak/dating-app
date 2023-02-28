using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    //LoginDTO is used to store data on login
    public class LoginDTO
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}