using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    //Seed class is used to create the database seed
    public class Seed
    {
        //clear connections is used to clear the chat group connections from the db
        public static async Task ClearConnections(DataContext context)
        {
            context.Connections.RemoveRange(context.Connections);
            await context.SaveChangesAsync();
        }

        //seed the users in the database
        public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            //if there are some records in the database, return
            if (await userManager.Users.AnyAsync()) return;


            //add user roles in the database
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




            //read users data from the file
            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

            //set options
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            //deserialize the data from the file
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

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
                //add user to the specific role group "Member"
                await userManager.AddToRoleAsync(user, "Member");
            }

            //create admin user
            var admin = new AppUser
            {
                UserName = "admin"
            };
            //create admin user with password "admin1"
            await userManager.CreateAsync(admin, "admin1");
            //set the user roles for the admin
            await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
        }

    }
}