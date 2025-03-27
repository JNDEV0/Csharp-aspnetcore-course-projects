
namespace AspCoreSection4
{
    public class ShorthandConventionalMw : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            //custom logic, including custom methods in this class can be called
            //before and after the next middleware, chaining middleware operations
            await context.Response.WriteAsync("Middleware class logic, starts.\n");

            //calling next middleware
            await next(context);

            //execution returns here after the next middleware is resolved
            await context.Response.WriteAsync("Middleware class logic, ends.\n");
        }
    }

    //the conventional way of making custom middleware class
    //where the IMiddleware interface is not implemented or injected into the app
    //instead the next request delegate is passed when the custom class is instantiated, and context is passed to Invoke method when called by app
    public class ConventionalMw
    {
        RequestDelegate _next;

        public ConventionalMw(RequestDelegate next) 
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            //custom logic example operation,
            //read the query for first and last name key pairs, assign it into memory as fullName and add it to responsy body.
            string fullName;
            if (context.Request.Query.ContainsKey("firstName") && context.Request.Query.ContainsKey("lastName"))
            {
                fullName = $"{context.Request.Query["firstName"]} {context.Request.Query["lastName"]}";
                await context.Response.WriteAsync($"{fullName}\n");
            }

            //calling next middleware
            //by making this method async, the execution can continue AFTER the next middleware completes
            //continuing logic in this method below, without async this would have to be returned.
            await _next(context);

            //additional custom logic will execute after completion of above called next middleware
        }
    }

    public static class MwExtension
    {
        public static IApplicationBuilder UseShorthandCustomNw(this IApplicationBuilder app)
        {
            //the IApplicationBuilder is essentially being injected with this custom code of the custom middleware class's entry point InvokeAsync
            //before being built, and without running the logic, sitting by available
            //as something that can then be called after its built, in desired order
            return app.UseMiddleware<ShorthandConventionalMw>();
        }
    }
}
