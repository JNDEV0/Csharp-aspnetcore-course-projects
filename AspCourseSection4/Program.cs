//middleware are series of operations to be performed on a request.
//middleware can be a lambda expression or a class for that middleware
//the request does not need to be passed from one middleware to the next, terminal or short circuit middleware

using AspCoreSection4;

var builder = WebApplication.CreateBuilder(args);

//before the app is built, custom middleware class needs to be added
//using AddTransient method to the Services that will be built using dependency injection to the app being built
//there are many other types, example AddSingleton
builder.Services.AddTransient<ShorthandConventionalMw>();
var app = builder.Build();

//middleware must call next(context) with async await to for an operation chain
//the middleware that exits before passing to other middleware is the terminal operation
app.Use(async (HttpContext RequestContext, RequestDelegate nextMiddleware) => 
{
    await RequestContext.Response.WriteAsync("middleware 1\n");
    nextMiddleware(RequestContext);
});

//the custom middleware class is passed as generic type to UseMiddleware,
//internally UseMiddleware will call the async Task InvokeAsync method of the middleware class,
//that receives the HttpContext and RequestDelegate/nextMiddleware parameters
//so instead of the method being a lambda expression as above, all the logic can be placed inside the middleware class
app.UseMiddleware<ShorthandConventionalMw>();

//Use is the convention of naming so this could be UseCustomRouting, UseCustomAuthenticationLogic etc
//this Custom Method was injected into the appplication builder by creating a method that receives an interface of the app builder
//so now it can be called from the built webapp. see Midleware.cs for custom extension class
//the injected built custom method can then call other logic, including middleware classes
//this middleware is made to use the same logic of the above custom Middleware Mw as demonstration.
app.UseShorthandCustomNw();

//this custom middleware class could NOT be injected before the build method
//because in the conventional manner IMiddleware is not implemeneted
//still, the custom middleware can be called directly specifying as generic type with UseMiddleware()
//see Middleware.cs for custom extension methods static class
app.UseMiddleware<ConventionalMw>();

//if first lambda predicate condition returns true,
//in this case checking if the url contains a query named username,
//then execute second lambda within the app, which can call any of the various middleware methods of app
app.UseWhen(context => context.Request.Query.ContainsKey("username"),
    app =>
    {
        app.Use(async (context, next) =>
        {
            await context.Response.WriteAsync("Hello from UseWhen, username is in query");

            //note that here next is being called, but will only execute if the condition is met
            //so if a request is not sent with a username query, the next middlewares will be skipped.
            await next();
        });
    });

//this middleware will execute, IF it is called above
//a middleware class implements IMiddleware
//which will have an async method called InvokeAsync, which is called by app.Use instead of the lambda expression
//the middleware class can also call next middleware and continue its own execution like a lambda expression
//the middleware class is just to compartmentalize logic that may be too long, specialized.
app.Use(async (HttpContext RequestContext, RequestDelegate nextMiddleware) =>
{
    await RequestContext.Response.WriteAsync("middleware 2\n");
});

//will not execute since it is not called by above middleware
app.Use(async (HttpContext RequestContext, RequestDelegate nextMiddleware) =>
{
    await RequestContext.Response.WriteAsync("middleware 3\n");
});

app.Run();
