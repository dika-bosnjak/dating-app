using API.Data;
using API.Entities;
using API.Extensions;
using API.Extentions;
using API.Middleware;
using API.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityService(builder.Configuration);
var app = builder.Build();

//use user-defined exception middleware
app.UseMiddleware<ExceptionMiddleware>();

//use cors (user-defined origins)
app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowCredentials()
.WithOrigins("http://localhost:4200", "https://localhost:4200"));

//use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

//map the controllers
app.MapControllers();

app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");

app.MapFallbackToController("Index", "Fallback");

//create the app scope and set the services
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    //get the app context
    var context = services.GetRequiredService<DataContext>();
    //do the db migrations
    await context.Database.MigrateAsync();
    //seed the database
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();

    await context.Database.ExecuteSqlRawAsync("DELETE FROM [Connections]");
    await Seed.SeedUsers(userManager, roleManager);
}
catch (Exception ex)
{
    //log the errors during the database migration and seed
    var logger = services.GetService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration");
}

// run the application
app.Run();
