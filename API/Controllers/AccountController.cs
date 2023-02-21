using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public IUserRepository _userRepository { get; }
        private readonly IMapper _mapper;

        public AccountController(DataContext context, ITokenService tokenService, IUserRepository userRepository, IMapper mapper)
        {
            _mapper = mapper;
            _tokenService = tokenService;
            _context = context;
            _userRepository = userRepository;
        }

        //register function
        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            //check whether there is a user in the database with the specific username (whether the username is already taken)
            if (await UserExists(registerDTO.Username)) return BadRequest("Username is taken");

            //map the registerDTO data to app user object
            var user = _mapper.Map<AppUser>(registerDTO);

            //set username to lower case
            user.UserName = registerDTO.Username.ToLower();

            //use hmac and create password hash and salt
            using var hmac = new HMACSHA512();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
            user.PasswordSalt = hmac.Key;

            //add the user in the db
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            //return the user data (username, token, known as and gender)
            return new UserDTO
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        //login function
        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            //check whether there is a user in the database with the specific username
            var user = await _userRepository.GetUserByUsernameAsync(loginDTO.Username);
            if (user == null) return Unauthorized("invalid username");

            //use hmac and compute hash from the inserted password
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

            //loop through the computedHash and compare it to the stored password hash
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("invalid password");
            }

            //return the user data (username, token, main photo, known as and gender)
            return new UserDTO
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        //userExists check whether there is a user with the specific username (username must be unique)
        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}