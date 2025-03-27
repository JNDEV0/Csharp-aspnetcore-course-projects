using Microsoft.AspNetCore.Mvc;
using PartialViews.Models;

namespace PartialViewsExample.Controllers
{
    public class HomeController : Controller
    {
        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("about")]
        public IActionResult About()
        {
            return View();
        }

        /* just as the entire View can be returned from the controller actionmethod to the response to browser
         * a partial view can be returned to the browser, whats the point?
         * a page loads a basic view to load a page strcture quickly, then asyncronously requests
         * partial views of the page components that need to be updated. while this may generate more
         * server requests, a partial view can be model bound and returned WITHOUT refreshing the entire view
         * so a stock price ticker on a blog could refresh the price without updating/reloading the entire page
         */
        [Route("languages")]
        public IActionResult Languages()
        {
            return View();
        }

        [Route("languages/get")]
        public IActionResult GetProgLangs()
        {
            //this model object is not typically created in the controller,
            //but here for brevity a new Model has been created in /Models folder
            //and instantiated here then passed to the partial view
            //the partialView is constructed to output the content
            //of the model in an async way, so the languages page has a button
            //that will call on languages/get to load this content below from server to client
            //dynamically without reloading the entire view page
            ProgListModel progLangs = new ProgListModel(
                "Programming Languages List", 
                new List<string>()
                {
                    "C#",
                    "Javascript",
                    "Go"
                });

            return PartialView("_ListPartialView2", progLangs);
        }
    }
}