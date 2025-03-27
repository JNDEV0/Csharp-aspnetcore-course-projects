using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MinimalApiExample.EndpointFilters;
using MinimalApiExample.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MinimalApiExample.RouteGroups
{
    //CustomMapGroup is used to group related routes in MinimalAPI, and .ProductsAPI() is called from the mapGroup in Program.cs
    //this allows containerizing the route logic away from Program.cs similar to controllers that would have a [Route()] tag for all contained routes
    //useful when despite the term MinimalAPI, there are in fact many routes.
    public static class ProductsMapGroup
    {
        //a mockList of products, in production this would be retrieved from a database
        public static List<Product> products = new List<Product>()
        {
            new Product(1, "toaster"),
            new Product(2, "hairDrier"),
            new Product(3, "copperTubing")
        };

        //instead of all individual route logic being directly added in Program.cs,
        //an extension method is given and called from the mapped/assigned group,
        //requests are then routed to this extension method based on the given route
        public static RouteGroupBuilder ProductsAPI(this RouteGroupBuilder routeGroupBuilder)
        {
            //IResult is the base interface implemented by different Results. types which can be returned by MinimalAPI
            //Results.Ok() 200, .Json(), .Text(), .File(), .BadRequest() 400, .NotFound() 404, .Unauthorized() 401,
            //each return type usually has a default response title as well as assign their corresponding status codes automatically
            //.ValidationProblem() 400 creates json with validationErrors collection, these are compiled from model binding property annotations
            //note the return types can receive various param types as well, string message, an object that is json serialized with KVPs
            //returning an IResult type enforces IResult return type for the entire lambda expression, so NOT using them may be useful as well

            //note that mapGroup.MapGet is used, so the route is actually /products
            routeGroupBuilder.MapGet("/", async (HttpContext requestContext) =>
            {
                
                var content = string.Join('\n', products.Select(temp => temp.ToString()));
                await requestContext.Response.WriteAsync(content);
            });

            //note the :int type for the parameter, optional
            routeGroupBuilder.MapGet("/{id:int}", async (HttpContext requestContext, int id) =>
            {
                Product? product = products.FirstOrDefault(temp => temp.Id == id);
                if (product is null)
                {
                    //requestContext.Response.StatusCode = 400;
                    //await requestContext.Response.WriteAsync("product id not found");
                    //return;

                    //equivalent to the above lines of code, to show how the Results.types work
                    return Results.BadRequest("product id not found");
                }

                //to demonstrate that in minimal API the data returned is up to implementation,
                //unlike a full MVC server that is expected to return HTTP responses
                //a compiled string, a serialized json object, an int, whatever data type.
                //await requestContext.Response.WriteAsync(product.ToString());
                //await requestContext.Response.WriteAsync(JsonSerializer.Serialize(product));
                
                return Results.Json(product);
            });

            //model validation through property annotations in expected inbound request datamodel
            //and being mapGroup bound, the route is /products/add
            routeGroupBuilder.MapPost("/add", async (HttpContext requestContext, Product product) =>
            {
                //note that MinimalApi does not use model binding that takes place in MVC or webAPI servers, so no ModelState.IsValid
                //validation is implemented using IEndpointFilters in MinimalAPI, still using [annotations] in the target Model class

                products.Add(product);

                //await requestContext.Response.WriteAsync("Post request received");
                return Results.Ok("Post request received");

                //an EndpointFilter can be added directly after a given route MapPost, MapGet etc,
                //THIS codeblock inside the mapped lambda is considered the delegateFunction, and incoming requests
                
                //!!IMPORTANT
                //endpointfilters dramatically change the route of incoming requests
                //requests are routed to endpointFilters FIRST in FIFO ORDER, eventually next(context) is called from within the endpointfilter stack
                //without any further filters pending in the stack, and THEN the "delegate"/this mapped lambda function is called, once Results.Ok()
                //is returned above, the request actually returns BACK INTO the endpointFilter stack below at LIFO ORDER for postlogic operations
                //that is, postlogic returns in reverse order to the request arrival, postlogic is in reverse order, so:
                //requestIncoming->firstEndpointFilterPreNext->lastEndpointFilterPreNext->delegate->lastEndpointFilterPostNext->firstEndpointFilterPostNext
                //without endpointfilters, only this codeblock would be accessed.

                //aside from directly implementing the lambda inside AddEndpointFilter() as commented out below,
                //a customEndpointFilter : IEndpointFilter class can be implemented
                //with an InvokeAsync method to separate the endpointFilter's logic to a separate class file
                //which also enables the endpointFilter to be reused in multiple mapped routes
                //remembering that one mapped route can have multiple associated endpointFilters in a call stack
                //ProductEndpointFilter type is passed to AddEndpointFilter to point to custom class described above
            })
                .AddEndpointFilter<ProductEndpointFilter>()
            //    .AddEndpointFilter(async (EndpointFilterInvocationContext context, EndpointFilterDelegate next) =>
            //{
            //    //context.HttpContext can view request context, perhaps for header or jwt validation, etc
            //    Product? product = context.Arguments.OfType<Product>().FirstOrDefault();
            //    if (product is null) return Results.BadRequest("product details missing or in wrong format");

            //    var validationContext = new ValidationContext(product);
            //    List<ValidationResult> validationResult = new List<ValidationResult>();

            //    //the fourth param true indicates validate all properties of model class
            //    bool isValid = Validator.TryValidateObject(product, validationContext, validationResult, true);
            //    if (isValid == false)
            //    {
            //        Dictionary<string, string[]> temp = new Dictionary<string, string[]>();
            //        foreach (var item in validationResult)
            //        {
            //            temp.Add(item.MemberNames.First(), new string[] { item.ErrorMessage } );
            //        }
            //        return Results.ValidationProblem(temp);
            //    }

            //    //next(context) invokes and current request to next endpointFilter, OR
            //    //if there are no further filters, it calls the "delegate" which is the lambda inside the .MapPost attached above
            //    //note the delegate function does NOT execute first when an endpointFilter is applied,
            //    //so incoming Post requests to above /add route will fall into this codeblock before products.Add() is called
            //    //thats why validation is performed in endpointfilter, if the logic reaches this point without returning
            //    //considering validation passed
            //    var result = await next(context);

            //    //this space is considered for post operation logic, that is, after validation above, and the next() call
            //    //which finds no further endpointfilters so passes to main delegate function, which will in turn return EndpointFilterDelegate
            //    //and CONTINUES HERE since the next() call is not returned directly above

            //    return result;
            //})
                ;

            //update/put request
            //note the param tags work here as well, similar to actionMethods in controllers
            routeGroupBuilder.MapPut("/{id}", async (HttpContext requestContext, [FromBody] Product product) =>
            {
                Product? retrievedProduct = products.FirstOrDefault(p => p.Id == product.Id);
                if (retrievedProduct is null)
                {
                    //requestContext.Response.StatusCode = 400;
                    //await requestContext.Response.WriteAsync("product id to update is not found");
                    //return;
                    return Results.BadRequest("product id to update is not found");
                    
                }

                //very simple just updating the mockData collection, in production the data would be retrieved from database,
                //changes identified assigned and updated to database provider
                retrievedProduct.Name = product.Name;
                
                //await requestContext.Response.WriteAsync("Product updated");
                return Results.Ok("Product updated");
            });

            routeGroupBuilder.MapDelete("/{id}", async (HttpContext requestContext, int id) =>
            {
                Product? retrievedProduct = products.FirstOrDefault(p => p.Id == id);
                if (retrievedProduct is null)
                {
                    requestContext.Response.StatusCode = 400;
                    await requestContext.Response.WriteAsync("product id to delete is not found");
                    return;
                }

                //very simple just updating the mockData collection, in production the data would be retrieved from database,
                //changes identified assigned and updated to database provider
                products.Remove(retrievedProduct);

                //returning an IResult type enforces IResult return type for the entire lambda expression, so NOT using them may be useful as well
                //here the productIsNull check does not return a BadRequest() as above routes, and so the expression below can be used to
                //write to the eventual Response without explicitly returning it as a Results.type
                await requestContext.Response.WriteAsync("Product deleted");
            });

            return routeGroupBuilder;
        }


    }
}
