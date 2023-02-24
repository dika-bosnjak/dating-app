using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly ITokenService _tokenService;

        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _uow;

        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _userManager = userManager;
            _mapper = mapper;
            _tokenService = tokenService;
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

            //add the user in the db
            var result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");

            if (!roleResult.Succeeded) return BadRequest(result.Errors);

            //return the user data (username, token, known as and gender)
            return new UserDTO
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        //login function
        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            //check whether there is a user in the database with the specific username
            var user = await _uow.UserRepository.GetUserByUsernameAsync(loginDTO.Username);
            if (user == null) return Unauthorized("Invalid username");

            var result = await _userManager.CheckPasswordAsync(user, loginDTO.Password);

            if (!result) return Unauthorized("Invalid password");

            //return the user data (username, token, main photo, known as and gender)
            return new UserDTO
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        //userExists check whether there is a user with the specific username (username must be unique)
        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}