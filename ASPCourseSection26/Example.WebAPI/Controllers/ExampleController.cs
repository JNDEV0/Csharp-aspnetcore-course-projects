using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

//a WebAPI is intended to return data, unlike a regular MVC controller that compiles views
//note that launchsettings default profile will set the string default url per profile, should point to api/exampleapi here
namespace CitiesManager.WebAPI.Controllers
{
    //the default webAPI empty controller shows a few conventions and requirements:
    //the route points to api/ by convention
    //the ApiController tag points out this is an API controller not a regular MVC controller
    //the ControllerBase parent class is used by API, since Controller class also inherits ControllerBase and adds view related functionality like viewbag, viewdata etc, and ControllerBase does not
    //[Route("api/[controller]")] //api/exampleapi
    //[ApiController]
    //see CustomControllerBase, this controller inherits [Route] and [ApiController] tags from CustomControllerBase class
    public class ExampleController : CustomControllerBase //ControllerBase //-Controller suffix == [controller] in route tag
    {

        //recommended to name the action method with relevance to its purpose, but is not requirement, actionmethod can be named anything, the [] tag defines what its for, GET, PUT, DELETE etc
        [HttpGet]
        public string Get()
        {
            //note that webAPI is intended to return data values, here a string, can be other data like int or bool, but NOT a razorpages view/html.
            //the same program can have api data routes and other view routes in the same solution as well
            return "hello world";
        }
    }
}
