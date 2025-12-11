using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ZhooSoft.Tracker.Common
{
    public class ServiceAuthAttribute : Attribute, IAsyncActionFilter
    {
        #region Methods

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var expectedSecret = config["ServiceAuth:SharedSecret"];
            var actualSecret = context.HttpContext.Request.Headers["X-Service-Auth"].FirstOrDefault();

            if (string.IsNullOrEmpty(actualSecret) || actualSecret != expectedSecret)
            {
                context.Result = new UnauthorizedObjectResult("Invalid service authentication.");
                return;
            }

            await next();
        }

        #endregion
    }

}
