using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Webapi.Server.Filters
{
    public class ActionParameterFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.Result == null)
            {
                if (!context.ModelState.IsValid)
                {
                    context.Result = BadRequest(context);
                    return;
                }
            }
        }


        public void OnActionExecuted(ActionExecutedContext context)
        {

        }
        
        IActionResult BadRequest(ActionExecutingContext context)
        {
#if DEBUG
            var result = new BadRequestObjectResult(context.ModelState);
            result.ContentTypes.Clear();
            result.ContentTypes.Add("application/vnd.error+json");
            return result;
#else
            return new BadRequestResult();
#endif
        }
    }
}
