using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Turbo.Net
{
    public class TurboMiddleware : IMiddleware
    {
        public const string TurboLocationHeader 
            = "Turbo-Location";

        public async Task InvokeAsync(
            HttpContext httpContext, 
            RequestDelegate next
        )
        {
            httpContext.Response.OnStarting((state) => {
                if (state is HttpContext ctx) 
                {
                    if (ctx.IsTurboRequest())
                    {
                        ctx.Response.Headers.Add(TurboLocationHeader, ctx.Request.GetEncodedUrl());
                    }
                }

                return Task.CompletedTask;
            }, httpContext);
            
            await next(httpContext);
        }
    }
}

