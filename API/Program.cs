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

//set the connection string for the database
var connString = "";
if (builder.Environment.IsDevelopment())
    connString = builder.Configuration.GetConnectionString("DefaultConnection");
else
{
    // Use connection string provided at runtime by FlyIO.
    var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    // Parse connection URL to connection string for Npgsql
    connUrl = connUrl.Replace("postgres://", string.Empty);
    var pgUserPass = connUrl.Split("@")[0];
    var pgHostPortDb = connUrl.Split("@")[1];
    var pgHostPort = pgHostPortDb.Split("/")[0];
    var pgDb = pgHostPortDb.Split("/")[1];
    var pgUser = pgUserPass.Split(":")[0];
    var pgPass = pgUserPass.Split(":")[1];
    var pgHost = pgHostPort.Split(":")[0];
    var pgPort = pgHostPort.Split(":")[1];

    connString = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};";
}
//set the db context to use postgre sql
builder.Services.AddDbContext<DataContext>(opt =>
{
    opt.UseNpgsql(connString);
});

var app = builder.Build();

//use user-defined exception middleware
app.UseMiddleware<ExceptionMiddleware>();

//use cors (user-defined origins)
app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowCredentials()
.WithOrigins("http://localhost:4200", "https://localhost:4200"));

//use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

//use default and static files (for wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

//map the controllers
app.MapControllers();

//map the hubs for presence and messages
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");

//map the fallback controller
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

    //clear the connections from the db
    await Seed.ClearConnections(context);
    //seed the users in the db
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
