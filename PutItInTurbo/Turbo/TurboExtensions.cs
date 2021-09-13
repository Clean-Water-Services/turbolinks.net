using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

namespace Turbo.Net
{
    public static class TurboExtensions
    {
        public static IApplicationBuilder 
            UseTurbo(this IApplicationBuilder app) => 
            app.UseMiddleware<TurboMiddleware>();

        public static IServiceCollection
            AddTurbo(this IServiceCollection serviceCollection) =>
            serviceCollection.AddSingleton<TurboMiddleware>();

        public static bool IsTurboRequest(this HttpContext ctx) =>
            ctx.Request.Headers.ContainsKey("Turbo-Referrer");

        public static bool IsXhrRequest(this HttpContext ctx) =>
            ctx.Request.Headers.TryGetValue("X-Requested-With", out var result) && 
            result == "XMLHttpRequest";
        
        public static IActionResult TurboRedirectToPage(this PageModel model, 
            string pageName,
            string pageHandler = null,
            object values = null,
            string protocol = null,
            TurboActions turboAction = TurboActions.Active)
        {
            var url = model.Url.Page(pageName, pageHandler, values, protocol);
            return new TurboRedirectResult(url, turboAction);
        }
        
        public static IActionResult TurboRedirectToAction(this PageModel target, 
            string action, 
            string controller,
            object values = null,
            TurboActions turboAction = TurboActions.Active)
        {
            var url = target.Url.Action(action, controller, values);
            return new TurboRedirectResult(url, TurboActions.Active);
        }
        
        public static IActionResult TurboRedirectToAction(this PageModel target, 
            string action, 
            object values = null,
            TurboActions turboAction = TurboActions.Active)
        {
            var url = target.Url.Action(action, values);
            return new TurboRedirectResult(url, turboAction);
        }
        
        public static IActionResult TurboRedirectToPage(this Controller model, 
            string pageName, 
            string pageHandler = null, 
            object values = null,
            string protocol = null,
            TurboActions turboAction = TurboActions.Active)
        {
            var url = model.Url.Page(pageName, pageHandler, values, protocol);
            return new TurboRedirectResult(url, turboAction);
        }

        public static IActionResult TurboRedirectToAction(this Controller target, 
            string action, 
            string controller,
            object values = null,
            TurboActions turboAction = TurboActions.Active)
        {
            var url = target.Url.Action(action, controller, values);
            return new TurboRedirectResult(url, turboAction);
        }
        
        public static IActionResult TurboRedirectToAction(this Controller target, 
            string action, 
            object values = null,
            TurboActions turboAction = TurboActions.Active)
        {
            var url = target.Url.Action(action, values);
            return new TurboRedirectResult(url, turboAction);
        }
    }
}