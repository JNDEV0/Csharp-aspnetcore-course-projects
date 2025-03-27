
using MinimalApiExample.Models;
using System.ComponentModel.DataAnnotations;

namespace MinimalApiExample.EndpointFilters
{
    //CustomEndpointFilter 
    public class ProductEndpointFilter : IEndpointFilter
    {
        private readonly ILogger<ProductEndpointFilter> _logger;

        //Dependency Injected service example, service classes that implement clean architecture and are added to builder.Services
        //same as webAPI or MVC server, can be injected into endpointfilters same as a controller class
        //ie in program.cs => builder.Services.AddScoped<IMyService, MyService>(); allows injecting IMyService
        public ProductEndpointFilter(ILogger<ProductEndpointFilter> logger)
        {
            _logger = logger;
        }

        //ValueTask is for returning the "request delegate" or next endpoint filter in the filter stack
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            //context.HttpContext can view request context, perhaps for header or jwt validation, etc
            //note that this code is the same code inside ProductEndpointFilter, to demonstrate two ways of adding filters to maproutes
            Product? product = context.Arguments.OfType<Product>().FirstOrDefault();
            if (product is null) return Results.BadRequest("product details missing or in wrong format");

            var validationContext = new ValidationContext(product);
            List<ValidationResult> validationResult = new List<ValidationResult>();

            //the fourth param true indicates validate all properties of model class
            bool isValid = Validator.TryValidateObject(product, validationContext, validationResult, true);
            if (isValid == false)
            {
                Dictionary<string, string[]> temp = new Dictionary<string, string[]>();
                foreach (var item in validationResult)
                {
                    temp.Add(item.MemberNames.First(), new string[] { item.ErrorMessage });
                }
                return Results.ValidationProblem(temp);
            }

            //next(context) invokes and current request to next endpointFilter, OR
            //if there are no further filters, it calls the "delegate" which is the lambda inside the .MapPost attached above
            //note the delegate function does NOT execute first when an endpointFilter is applied,
            //so incoming Post requests to above /add route will fall into this codeblock before products.Add() is called
            //thats why validation is performed in endpointfilter, if the logic reaches this point without returning
            //considering validation passed
            //calling next(context) invokes the next endpoint filter
            //or the request delegate if there are no more filters in the stack
            var result = await next(context);

            //after logic is triggered only after the call stack returns here,
            //this space is considered for post operation logic, that is, after validation above, and the next() call
            //which finds no further endpointfilters so passes to main delegate function, which will in turn return EndpointFilterDelegate
            //and CONTINUES HERE since the next() call is not returned directly above
            return result;
        }
    }
}
