using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        public UserManager<AppUser> _userManager { get; }
        public AdminController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;

        }

        //admin route - only admin can see users with theirs roles
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            //get the users (id, username, role) from the database
            var users = await _userManager.
            Users
            .OrderBy(u => u.UserName)
            .Select(u => new
            {
                u.Id,
                Username = u.UserName,
                Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
            }).ToListAsync();

            //return users
            return Ok(users);
        }

        //admin route - only admin can update users roles
        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            //check whether any role is selected
            if (string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");

            //separate the selected roles
            var selectedRoles = roles.Split(",").ToArray();

            //find the selected user in the database
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return NotFound();

            //get users old roles
            var userRoles = await _userManager.GetRolesAsync(user);

            //add user to the new roles
            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if (!result.Succeeded) return BadRequest("Failed to add to roles");

            //remove user from the roles that are not selected
            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!result.Succeeded) return BadRequest("Failed to remove from roles");

            //return the current user roles
            return Ok(await _userManager.GetRolesAsync(user));

        }
    }
}