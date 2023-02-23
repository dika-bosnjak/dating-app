
using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Extentions
{
    //ApplicationServiceExtensions - used to simplify Program.cs file (all manually created services are here)
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            //database
            services.AddDbContext<DataContext>(opt =>
            {
                opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });

            //CORS
            services.AddCors();

            //Token service
            services.AddScoped<ITokenService, TokenService>();

            //User repository
            services.AddScoped<IUserRepository, UserRepository>();

            //User acitivity logger
            services.AddScoped<LogUserActivity>();

            //Mapper (entities to DTO)
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            //Like repository
            services.AddScoped<ILikesRepository, LikesRepository>();

            //Message repository
            services.AddScoped<IMessageRepository, MessageRepository>();

            //SingalR
            services.AddSignalR();
            services.AddSingleton<PresenceTracker>();

            return services;
        }

    }
}