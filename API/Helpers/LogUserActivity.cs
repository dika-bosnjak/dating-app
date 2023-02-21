using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers
{
    //LogUserActivity class is used to track user activity and to modify last active property in the db
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //get the context and check whether the user is authenticated
            var resultContext = await next();
            if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

            //get the user id from the token (claim)
            var userID = resultContext.HttpContext.User.GetUserId();

            //get the user repository
            var repo = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();

            //get the user and modify his active datetime
            var user = await repo.GetUserByIdAsync(userID);
            user.LastActive = DateTime.UtcNow;
            await repo.SaveAllAsync();
        }
    }
}