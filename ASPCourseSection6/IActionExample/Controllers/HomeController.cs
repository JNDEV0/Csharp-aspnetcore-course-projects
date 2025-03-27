using Microsoft.AspNetCore.Mvc;

namespace IActionStatusCodesExample.Controllers
{
    public class HomeController : Controller
    {
        //note that Content and File are different return types within the same action method below, this only works in the same action method because of the parent IActionResult return type that can receive multiple return types
        //one issue with route constraints is that if the validation does not match, the action method is simply skipped, sometimes this is desired, but it does not allow conditional handling of the response
        //perhaps there are two or three route variable constraints, and one of them is missing or has a typo. validating the variables individually inside a more generalized route allows the method to still handle the request
        //in this example, instead of using route constraints, only the base route is stipulated, so website.com/books/?* passes into this initial route validation, where ?* means any query key value pair passed, will need to be verified inside the action method
        [Route("bookstore")]
        public IActionResult Index()
        {
            //note that in this method, there are multiple return types, Content strings and File pdf, because of this, neither ContentResult or FileResult can be used, since it would not compile with multiple return types. So the parent can be used, and is recommended by default, IActionResult

            //validating if query is even present and has a value
            //immediatelly returning the error message instead of handling bad request
            //if this validation of variables were done in the Route, this code block wouldve been skipped entirely and no custom message couldve been sent from here, it wouldve had to be handled by other middleware, so this option is good for contextual handling of requests
            //even though any key value pair can be provided in the url query, it will only ever make it past this point if the correct one is provided
            if (!Request.Query.ContainsKey("isloggedin"))
            {
                //status codes are a preset of number codes that signal outcomes from server to client, 200 is default OK response
                //there are some prebuilt classes that can represent common results, which will automatically set the status code
                //the return below is the same as these two lines:
                //Response.StatusCode = 400;
                //return Content("missing login status.");
                //another way is instantiating
                //return new BadRequestResult("missing login status.");
                //another way is calling this controller method
                return BadRequest("missing login status.");
            }
            else
            {
                try
                {
                    if (Convert.ToBoolean(Request.Query["isloggedin"]) is false)
                    {
                        //Response.StatusCode = 401;
                        //return Content("must be logged in");
                        return Unauthorized("must be logged in");
                    }
                }
                catch 
                {
                    //exception handling, what if someone passes an int instead of true or false?
                    //Response.StatusCode = 400;
                    //return Content("error reading isloggedin status");
                    //return BadRequest("error reading isloggedin status");
                    return StatusCode(400);
                }
            }

            //now that its validated that the user is "logged in", verify the bookid being accessed
            if (!Request.Query.ContainsKey("bookid"))
            {
                //Response.StatusCode = 400;
                //return Content("Book id not provided.");
                return BadRequest("Book id not provided.");
            }
            if (string.IsNullOrEmpty(Request.Query["bookid"]))
            {
                //Response.StatusCode = 404;
                //return Content("Book id empty.");
                return NotFound("Book id empty.");
            }

            //now that there is the bookid and it has some value assign to memory
            //note that this is the direct way of acceesing the request object
            int bookId = Convert.ToInt32(ControllerContext.HttpContext.Request.Query["bookid"]);

            //check if value is within acceptable range
            if (bookId < 1 || bookId > 1000)
            {
                //Response.StatusCode = 404;
                //return Content("book id must be 1-1000");
                return NotFound("book id must be 1-1000");
            }

            //to redirect from one url to another, in this case its redirecting website.com/bookstore/* to the actionmethod RedirectedToBookstore, that is in the Controller that starts with Store. Controller is not included because it will atuomatically be identified. the other route that is associated with the action method pointed to will be identified automatically
            //note the third argument, here any route parameter values such as bookstore/{bookid} would be passed along as id to the redirect target route
            //what this will do is send a response to the browser with status code 302 found, meaning redirect to another url path, and this new url path that the app identified is send in the headers of the response. the browser then normally makes a new request to the provided route

            //redirect to action is best used when the action method name and controller name, select variables want to be passed in the redirection, also controller and action method names can be stored in variables, further verifications, etc this option and permanent variant are often used
            //return new RedirectToActionResult("RedirectedToBookstore", "Store", new {}); //302 statuscode - found, redirect
            return RedirectToAction("RedirectedToBookstore", "Store", new { id = bookId });

            //the fourth parameter set to true, is a flag to respond with status code 301, which signals that this rerouting is permanent, perhaps for static content that has been reorganized into another path, see StoreController for more explanation
            //this redirection can be useful for something like performing all validaiton of the request in one route's action method, and then redirect to another route for business logic
           
            //return new RedirectToActionResult("RedirectedToBookstore", "Store", new { }, true); //301 statuscode - moved permanent, redirect
            return RedirectToActionPermanent("RedirectedToBookstore", "Store", new { id = bookId });

            //the local_url is the APPLICATION LOCAL url, a route within this same binary build, even if in another controller class
            //local redirect result simply requires the route to redirect to, including any variables to be passed along, may be useful when the route may actually be dynamically generated or is more complex than the example below
            return new LocalRedirectResult("store/books/{bookId}", false); //can be 301 or 302 depending on second param
            return LocalRedirect("store/books/{bookId}"); // 302
            return LocalRedirectPermanent("store/books/{bookId}"); // 301

            //RedirectResult is best used to redirect from one application to another, as in one server to another server in another domain entirely, perhaps a new domain is acquired, and people still acces the old one, or multiple typos of the website are purchased to redirect to the correct or new one, or perhaps linking a player after authentication to a load balanced game world server, determined at runtime which one is best for the player
            //this option can also be used for local redirect, but is less explicit and restrictive/specific than previous options
            return new RedirectResult("newdomain.com/home", true);
            return Redirect();
        }
    }
}
