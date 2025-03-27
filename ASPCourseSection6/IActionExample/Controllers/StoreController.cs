using Microsoft.AspNetCore.Mvc;

namespace IActionStatusCodesExample.Controllers
{
    public class StoreController : Controller
    {
        //in the example of this project, when website.com/bookstore/* is accessed, login and bookid is validaded, and then the browser is sent a redirect 302 found response to reroute to this page, which could be a post login page with further implementation
        //the status code can also be 301, telling the browser that this new route/actionmethod pair is the permanent new route, this boolean flag is set in the RedirectResponse returned from the method starting the redirect
        //the benefit of permanent redirection is that the local client browser as well as search engines will update their local cache data to automatically make the correct request directly to the redirected route/actionmethod, when the old path is called, instead of making two requests. this is good for SEO and also to avoid clients making unnecessary duplicate requests,
        //but statusCode 301 is not ideal for all situations, perhaps the redirected method is to be accessible only after some validation work happens on the first route, in which case 302 makes sense
        [Route("store/books/{id}")]
        public IActionResult RedirectedToBookstore()
        {
            //note that the route above could be acessed directly, or through the old route that redirects here, for simplicity sake in this example no further checks on the id are performed here

            //finally, if all validations pass without returning an error, returning the file, here just one sample file is returned, but could implement a library search function to actually correlate ids to multiple books
            //by default, StatusCode is 200 if none of the validations in the method that redirected here trigger first
            return File("/sample.pdf","application/pdf");
        }
    }
}
