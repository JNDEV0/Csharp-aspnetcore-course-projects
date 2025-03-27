/*
    UseRouting simply enables routing on the server.
    UseEndpoints actually then maps the "end points" or simply routes usually defined by URL components
    this is an older way of doing things, a newer way is discussed in section6 using controllers and action methods instead of endpoint mappings
    the custom constraint class, route variables and modifiers apply to both ways of processing requests/responses through the app server
 */
using RoutingServer.constraints;

var builder = WebApplication.CreateBuilder(args);

//custom constraints classes must be added to builder before it builds the app
//after adding the custim constraint in this way to the routing constraint map, it can be called as a variable type later
//for example, if the route is sales-report/{year:int}/{month:string} with custom constraint it can be {month:customMonth}
//this will execute the custom constraint class on validation of the route implementing it
//see line 160 for example usage
builder.Services.AddRouting(options => 
{ 
    options.ConstraintMap.Add("customMonth", typeof(MonthConstraint)); 
});

var app = builder.Build();

//note that enabling rounting will cause / root level URL requests return 404 until an endpoint is mapped for it
app.UseRouting();

//this middleware is storing the "Endpoint" from the request context data and storing it, then writing to the response
//the endpoint has some data, mainly the displayName may be useful
//as it contains the request type and URL path, used to identify which middleware to route the request
//getting the actual endpoint targeted by the request can be useful in some scenarios,
//like identifying where a user is headed from a home page, or processing requests by type before further middleware map routing
app.Use(async (context, next) => {
    Microsoft.AspNetCore.Http.Endpoint? endPoint = context.GetEndpoint();
    
    if (endPoint is not null)
    await context.Response.WriteAsync($"endpoint identified from URL path after / and request method: {endPoint.DisplayName}\n");
    
    await next(context);
});


//endpoints are short circuiting, meaning they function like a switch only matching endpoints are accessed
//endpoints must be configured with Mapping to specific middleware
//a "mapping" is a corellation between a URL route and a specific middleware to handle it
//the selection order of endpoints os more specific/hardcoded first, then variable options. order comes after this condition, so user/1 is chosen if available over user/{id:int}
app.UseEndpoints(endpoints =>
{
    endpoints.Map("endPoint1", async (context) =>
    {
        await context.Response.WriteAsync("since endPoint1 is in the URL, this endpoint is activated.");
    });

    //here a POST request is being mapped to the root / URL
    //if a GET, UPDATE, etc request is sent to this route, it will not be handled by this mapping
    endpoints.MapPost("/", async (context) =>
    {
        await context.Response.WriteAsync("Hello World!");
    });

    //here a variable is shown in the endpoint mapping, in this case the variable can be anything, but cannot be empty
    //while the literal part is required. so root/Files/anything.anything would pass the validation
    //parameter variables are provided as a dictionnary that holds the value as an object, that can be cast into a string, int etc.
    //note that fileNumber and fileExtension are not being defined as memory values in the endpoint condition check,
    //the parameter=value part is to assign a default value IN CASE the value is variable is empty,
    //in this example its implied that most files will probably be a .txt, there may be .jpg files too which would have to be specified in fileExtension
    endpoints.Map("Files/{fileNumber}.{fileExtension=txt}", async (context) =>
    {
        //validating the incoming details
        int incomingFileNumber = default;
        if (context.Request.RouteValues["fileNumber"] is not null)
        incomingFileNumber = Convert.ToInt32(context.Request.RouteValues["fileNumber"]);

        //note that here the is not null check is redundant, since the fileExtension has a default value of txt
        //even if no file extension is provided in the URL path
        string incomingFileExtension = default;
        if (context.Request.RouteValues["fileExtension"] is not null)
            incomingFileExtension = Convert.ToString(context.Request.RouteValues["fileExtension"]);

        //here the route values, or variables passed as parameters in the URL
        //after having been retrieved stored and cast
        //are being written to the response
        await context.Response.WriteAsync($"root/Files/{incomingFileNumber}.{incomingFileExtension} was accessed.");
    });

    //in this example, fileNumber and fileExtension can be null, meaning if nothing is provided the values will be null
    //and the request will STILL be associated the this mapping, so Files/. would reach this route same as Files/1.txt
    //also, fileNumber is given a required data type, if fileNumber is passed as "abc" for example, it will fail, must be 0-9+ number
    //route constraints are int, bool, datetime, decimal, long, quid various constraints can be set
    endpoints.Map("Files/{fileNumber:int?}.{fileExtension?}", async (context) =>
    {
        //incoming param data is given as string, needs to be converted to expected types
        int fileNumber = default;
        if (context.Request.RouteValues["fileNumber"] is not null)
            fileNumber = Convert.ToInt32(context.Request.RouteValues["fileNumber"]); 

        //since fileExtension can be null,  we are checking and assigning a default to the local variable if null
        string fileExtension = default;
        if (context.Request.RouteValues["fileExtension"] is not null)
            fileExtension = Convert.ToString(context.Request.RouteValues["fileExtension"]);
        else
            fileExtension = "txt";

        await context.Response.WriteAsync($"root/Files/{fileNumber}.{fileExtension} was accessed.");
    });

    //this datetime constraint requires that a date and time be provided example: 2030-12-31
    //if an invalid dateTime is provided, either in wrong format or invalid year/day/month number,
    //it will not pass the validation and will not execute this block
    endpoints.Map("daily-news/{reportDate:datetime}", async (context) =>
    {
        DateTime reportDate = default;
        if (context.Request.RouteValues["reportDate"] is not null)
            reportDate = Convert.ToDateTime(context.Request.RouteValues["reportDate"]);
        await context.Response.WriteAsync($"the daily news are good for: {reportDate}");
    });

    //in this example the route variable is required to be a valid guid format
    //guid is a hex value generated to be loosely unique, useful for temporary unique use like valid session key
    //note that this is not nullable, so if anything aside from a valid guid format, it will fail
    endpoints.Map("sessionKey/{hex:guid}", async (context) =>
    {
        Guid hex = default;
        bool success = default;
        if (context.Request.RouteValues["hex"] is not null)
            success = Guid.TryParse(Convert.ToString(context.Request.RouteValues["hex"]), out hex);

        if (success)
            await context.Response.WriteAsync($"the GUID hexadecimal provided: {hex}");
    });

    //this endpoint demonstrate length and alphanumeric variable constraints 
    endpoints.Map("employee-profile/{employeeName:length(2,15):alpha=default}", async (context) =>
    {
        string employeeName = "default";
        if (context.Request.RouteValues["employeeName"] as string != employeeName)
            employeeName = Convert.ToString(context.Request.RouteValues["employeeName"]);

        await context.Response.WriteAsync($"the employee name provided: {employeeName}");
    });

    //this example shows use of int, range and nullable variable constraints
    endpoints.Map("products/{id:int:range(1-999)?}", async (context) =>
    {
        int productId = default;
        if (context.Request.RouteValues["id"] is not null)
        {
            productId = Convert.ToInt32(context.Request.RouteValues["id"]);
            await context.Response.WriteAsync($"productId valid response: {productId}");
        }
        else
            await context.Response.WriteAsync($"invalid productId.");
    });

    //this example shows using min and regex validation
    endpoints.Map("sales-report/{year:int:min(2022)}/{month:regen(^(jan|feb|mar)$)}", async (context) =>
    {
        int year = Convert.ToInt32(context.Request.RouteValues["year"]);
        string month = Convert.ToString(context.Request.RouteValues["month"]);

        if (month == "jan")
            await context.Response.WriteAsync($"sales report for {month} {year}: 0$.");
        else
            await context.Response.WriteAsync($"no sales report for chosen month year.");
    });

    //this example shows using custom constraint class it is routing the route mapping validation to the custom class
    //added to the app builder as a route constraint map. the custom Constraint is essentially executing the same check
    //as the regex validaiton in the route above, but within an encapsulated logic class
    endpoints.Map("sales-report/{year:int:min(2022)}/{month:customMonth}", async (context) =>
    {
        int year = Convert.ToInt32(context.Request.RouteValues["year"]);
        string month = Convert.ToString(context.Request.RouteValues["month"]);

        await context.Response.WriteAsync($"sales report custom route constraint response for {month} {year} provided in URL.");

    });

    endpoints.Map("", async (context) =>
    {
        string x = default;
        if (context.Request.RouteValues["x"] is not null)
            x = Convert.ToString(context.Request.RouteValues["employeeName"]);

        await context.Response.WriteAsync($"a: {x}");
    });
});

//if any mapping is found, the endpoint mapping middleware will be called and the request is handled from there
//else if there are no endpoint mappings for the URL, execution continues below

//here the mapping is provided for GET request to root / URL route
//note that only POST and GET are accounted for in this program
//other request method types will result in a 405 until mapped to a middleware
app.MapGet("/", () => "Hello World!");

//a simple app.Run can be used as a final fallback for requests to paths not accounted for
//so for example website.com/aom9a42949gj2, something not mapped above, this app.Run below will still run
//note that if an endpoint is found, or even "/" root level access above, app.Run below will not be called
// since above middlewares are not calling next middleware
app.Run(async context =>
{
    await context.Response.WriteAsync($"bad URL path: {context.Request.Path}");
});

app.Run();
