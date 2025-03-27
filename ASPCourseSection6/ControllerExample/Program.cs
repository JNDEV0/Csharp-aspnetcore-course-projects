/*
 see the HomeController.cs file inside Controllers folder.
the template is MVC controller - empty.
 the "controller" is the C in MVC, model view controller pattern.
where http requests are received by the server, processed by middleware via endpoint routing, custom variable constraints, validation and custom logic to process the data, send CRUD operations to update database and send ammended responses back to the client. 
with controllers, the routing, is handled in the controller, which is a collection of routes and associated action methods, which receive the request when route validation passes, and returns a value. any value type can be returned, such as string, int, bool even custom class instances and "views" the V in MVC.
the controller allows separating the custom logic from the server program/main file, into neat methods that can have multiple routes associated with each, and custom variable constraints can still be used.

 */

var builder = WebApplication.CreateBuilder(args);

//the controller that contains the defined routes and related action methods must be "dependency injected" into the builder before calling Build()
//as a "service", which is basically a reusable class, which the app will identify as a "Controller"/handler class of requests
//AddContrllers() will detect classes with Controller appended to the end of the name, and add them to the builder
//this is because large projects may have many controllers, to automatically look for and add all of them this method simplifies the process
builder.Services.AddControllers();

//an alternate way to manually add a single Controller to the services
//builder.Services.AddTransient<HomeController>();

var app = builder.Build();

//static files are enabled here for file response example
app.UseStaticFiles();

//MapControllers() can be called, and it will call UseRouting and UseEndpoints, and go through all the Controller classes that AddControllers() has added to the now built application, and use those routes and action methods, instead of having to setup routing and mapping individually here
app.MapControllers();

//calling app.MapControllers essentially the same as:
//app.UseRouting();
//app.UseEndpoints(endpoints => {
//endpoints.MapControllers(); other mappings...; });

app.Run();
