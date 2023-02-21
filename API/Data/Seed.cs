using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    //Seed class is used to create the database seed
    public class Seed
    {
        public static async Task SeedUsers(DataContext context)
        {
            //if there are some records in the database, return
            if (await context.Users.AnyAsync()) return;

            //read usee data from the file
            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

            //set options
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            //deserialize the data from the file
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            //loop through all user data
            foreach (var user in users)
            {
                //use hmac (hash algorithm - Hash-based Message Authentication Code)
                using var hmac = new HMACSHA512();
                //set username to lower case
                user.UserName = user.UserName.ToLower();
                //create the password hash
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("password"));
                //create the password salt
                user.PasswordSalt = hmac.Key;
                //add the user to the context
                context.Users.Add(user);
            }

            //save all changes
            await context.SaveChangesAsync();
        }

    }
}