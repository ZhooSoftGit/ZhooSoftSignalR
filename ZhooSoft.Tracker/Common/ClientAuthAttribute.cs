﻿using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ZhooSoft.Tracker.Common
{
    public class ClientAuthAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var expectedSecret = config["ClientAuth:SharedSecret"];
            var actualSecret = context.HttpContext.Request.Headers["X-Service-Auth"].FirstOrDefault();

            if (string.IsNullOrEmpty(actualSecret) || actualSecret != expectedSecret)
            {
                context.Result = new UnauthorizedObjectResult("Invalid service authentication.");
                return;
            }

            await next();
        }
    }

}
