using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly UserManager<AppUser> _userManager;
        public TokenService(IConfiguration config, UserManager<AppUser> UserManager)
        {
            _userManager = UserManager;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
        }

        //create token method - create a token for a logged in user
        public async Task<string> CreateToken(AppUser user)
        {
            //create a claim with username
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
            };

            var roles = await _userManager.GetRolesAsync(user);

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            //create symmetric security key with sha512 alghoritm
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            //create token descriptor with its subject, expire date and signing credentials
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };

            //create token handler
            var tokenHandler = new JwtSecurityTokenHandler();

            //create token
            var token = tokenHandler.CreateToken(tokenDescriptor);

            //return token
            return tokenHandler.WriteToken(token);
        }
    }
}