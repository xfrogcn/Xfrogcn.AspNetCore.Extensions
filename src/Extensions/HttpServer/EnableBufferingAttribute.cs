using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class EnableBufferingAttribute : Attribute, IResourceFilter
    {
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            context.HttpContext.Request.EnableBuffering();
        }
    }
}
