using Microsoft.AspNetCore.Mvc;
using ModelBindingExample.Models;

namespace ModelBindingExample.Controllers
{
    public class HomeController : Controller
    {
        //for example, if the expected URL from client is: website.com/bookstore/?bookId=1&isLoggedIn=true
        //the order of data retrieval from the request is:
        //form fields, request body,
        //route variables if defined ie /{param1}/{param2},
        //then the query string ie: ?param1=value1&param2=value2
        //finally action method parameters if defined, otherwise ata will be in request body query data
        
        //the route parameter variables have precedence over the model bound(query string) parameters, and so are retrieved
        //model bound(actionmethod parameter) variables are retrieved from the request into local memory parameter variables
        //without needing to be specified in the route constraint, where route parameter variables need to be retrived from the request query data
        //aside from the default order of selection, there are methods to retrieve the values from specific source either data or query
        //the point of the selection order is that data is retrieved from the first priority option that contains the specification
        [Route("bookstore/{bookId?}/{isLoggedIn?}")]
        public IActionResult Bookstore([FromRoute]int? bookId, [FromRoute]bool? isLoggedIn)
        {
            //[FromRoute] and [FromQuery] specifies from which source should the action method assign the value to the parameter
            //in this example if bookId is NOT in the route data, EVEN if it is found in form fields, query string, headers anywhere else, bookId will have null value here.
            //so assigning a tag to the parameter specifies the source of the variable's value, even if other higher precedence sources are available
            //to clarify, for example: website.com/param1?param2=value where param1 is route data variable, and param2 is in query string, being that [FromRoute is specified, bookId would have value of param1 assuming its an int, and isLoggedIn would be null, since it is not present in the route variables, even if param2 in the query == isLoggedIn

            //validation
            //business logic
            return View();
        }

        //a reuqest to /view-books/1?isLoggedIn=true&bookId=10&AuthorName=exampleName
        //results in isLoggedin == null and the book object contains bookId == 10 and AuthorName == exampleName
        [Route("view-books/{bookId?}/{isLoggedIn?}")]
        public IActionResult Bookstore([FromRoute]bool? isLoggedIn, [FromQuery]BookSearch book)
        {
            //proeperty names are not case sensitive in query string route parameters or model binding, case sensitivity irrelevant
            //BookSearch model binding class, in this case book will point to an instantiated object of BookSearch with its fields populated from whichever source the data can be found or is specified, be it in form data, route data, query etc, which can then be utilized here
            //the [FromQuery] or [FromRoute] can be specified for the book class as a whole or for individual fields in the model class
            //the fields of the class however may be potentially null, so validation is needed such as if (isLoggedIn != null)... etc

            //validation
            //business logic
            return Content("Response result");
        }
    }
}
