
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

            //CORS
            services.AddCors();

            //Token service
            services.AddScoped<ITokenService, TokenService>();

            //User acitivity logger
            services.AddScoped<LogUserActivity>();

            //Mapper (entities to DTO)
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            //SingalR
            services.AddSignalR();
            services.AddSingleton<PresenceTracker>();

            //use unit of work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }

    }
}