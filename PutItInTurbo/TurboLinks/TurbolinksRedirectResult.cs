using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Turbo.Net
{
    public class TurboRedirectResult : RedirectResult
    {
        public TurboActions TurboAction { get; }

        public TurboRedirectResult(string url, TurboActions turboAction = TurboActions.Active) 
            : base(url)
        {
            TurboAction = turboAction;
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            var httpContext = context.HttpContext;
            if (httpContext.IsXhrRequest())
            {
                var action = TurboAction.ToString().ToLower();
                var content = httpContext.Request.Method == HttpMethods.Get
                    ? $"Turbo.visit('{this.Url}');"
                    : $"Turbo.clearCache();\nTurbo.visit('{this.Url}', {{ action: \"{ action }\" }});";

                var contentResult = new ContentResult {
                    Content = content,
                    ContentType = "text/javascript"
                };
                
                var executor = context
                    .HttpContext
                    .RequestServices
                    .GetRequiredService<IActionResultExecutor<ContentResult>>();
                
                return executor.ExecuteAsync(context, contentResult);
            }
            else
            {
                return base.ExecuteResultAsync(context);                
            }
        }
    }

    public enum TurboActions
    {    
        Active,
        Replace
    }
}