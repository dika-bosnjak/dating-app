using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    //Seed class is used to create the database seed
    public class Seed
    {
        public static async Task ClearConnections(DataContext context)
        {
            context.Connections.RemoveRange(context.Connections);
            await context.SaveChangesAsync();
        }

        public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            //if there are some records in the database, return
            if (await userManager.Users.AnyAsync()) return;

            //read usee data from the file
            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

            //set options
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            //deserialize the data from the file
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            var roles = new List<AppRole>
            {
                new AppRole{Name = "Member"},
                new AppRole{Name = "Admin"},
                new AppRole{Name = "Moderator"},
            };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }

            //loop through all user data
            foreach (var user in users)
            {
                //set username to lower case
                user.UserName = user.UserName.ToLower();
                //set utc date
                user.Created = DateTime.SpecifyKind(user.Created, DateTimeKind.Utc);
                user.LastActive = DateTime.SpecifyKind(user.LastActive, DateTimeKind.Utc);
                //add the user to the context
                await userManager.CreateAsync(user, "password");

                await userManager.AddToRoleAsync(user, "Member");
            }

            var admin = new AppUser
            {
                UserName = "admin"
            };

            await userManager.CreateAsync(admin, "admin1");
            await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
        }

    }
}