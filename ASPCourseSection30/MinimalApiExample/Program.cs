using MinimalApiExample.Models;
using MinimalApiExample.RouteGroups;
using System.Linq.Expressions;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

//a mapgroup applies to all routes grouped into it
//instead of app.MapGet("/products", ...); etc
//note the .ProductsAPI() call below is routing the /products/* requests to the ProductsMapGroup.cs,
//this is a MinimalAPI way of containerizing code away from program.cs, similar to a controller in MVC
var mapGroup = app.MapGroup("/products").ProductsAPI();

app.Run();
