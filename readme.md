# Turbo.NET

This is a sample [ASP.NET Core](https://asp.net) project working with [Hotwire Turbo](https://hotwired.dev).

> Turbo® makes navigating your web application faster. Get the performance benefits of a single-page application without the added complexity of a client-side JavaScript framework. Use HTML to render your views on the server side and link to pages as usual. When you follow a link, Turbo automatically fetches the page, swaps in its &lt;body&gt;, and merges its &lt;head&gt;, all without incurring the cost of a full page load.

## Changes between Turbolinks and Hotwire Turbo

* Renamed 'Turbolinks' to 'Turbo' everywhere.
* Used `turbo.es2017-esm.js` (from Turbo yarn build) as `wwwroot/lib/turbo/turbo.js`
* Updated `Pages/Shared/_Layout.cshtml` to load `turbo.js` as a module (type="module")

## TurboMiddleware

The `TurboMiddleware` adds a `Turbo-Location` header to responses, allowing Turbo the ability to manage **visits** of the client.

```c#
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
                if (ctx.IsTurboLinkRequest())
                {
                    ctx.Response.Headers.Add(TurboLocationHeader, ctx.Request.GetEncodedUrl());
                }
            }

            return Task.CompletedTask;
        }, httpContext);
        
        await next(httpContext);
    }
}
```

## TurboLinkRedirectResult

The `TurboLinkRedirectResult` class allows both **Razor Pages** and **ASP.NET MVC** to respond with a redirect that works with turbo. Turbo watches **Xhr** requests and will use the JavaScript snippets from the response to set state and clear cache.

```c#
public class TurbolinkRedirectResult : RedirectResult
{
    public TurboActions TurboAction { get; }

    public TurbolinkRedirectResult(string url, TurboActions turboLinksAction = TurboActions.Active) 
        : base(url)
    {
        TurboAction = turboLinksAction;
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
```

It is essential that the `POST` call happen via XHR as seen on the `Index` razor page.

```html
<form id="form" method="post" asp-controller="Values" asp-action="Index">
    <button class="btn btn-primary">
        Submit
    </button>
</form>

@section Scripts
{
    <script type="text/javascript">
     $(function () {
         
        $("#form").submit(function(e) {
            var action = $(this).attr("action");
            var data = $(this).serialize();
            var callback = function(data) {
                console.log(data);
            };
            
            $.post(action, data, callback);                 
            e.preventDefault();
        });
      });
    </script>    
}
```

We can use the `TurboRedirectResult` using extension methods.

### Razor Page Example

```c#
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }

    public bool HasName => !string.IsNullOrWhiteSpace(Name);
    public string Name { get; set; }

    public IActionResult OnPost()
    {
        return this.TurboLinkRedirectToPage("Privacy");
    }
}
```

### Controller Example

```c#
[Route("[controller]")]
public class ValuesController : Controller
{
    // Post
    [HttpPost, Route("")]
    public IActionResult Index()
    {
        return this.TurboLinkRedirectToPage("/Privacy");
    }
}
```

## Conclusion

This works and can be tweaked to support more complex return values for Turbo, but I found through basic testing that it is unnecessary for my use case.