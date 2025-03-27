//the controller is responsible for reading requests to validate the URL routes request bodies headers etc, invoke data model to handle business logic, calls the assigned handler method to prepare the response 

using ControllerExample.Models;
using Microsoft.AspNetCore.Mvc;

//projectname and folder name default,
//can be changed
namespace ControllerExample.Controllers
{
    //HoneController is the default name,
    //compiler recognizes classes appended with "Controller"
    //as Controller classes during compile, so customController, where custom is anything, is valid.
    //by default this class inherits from Controller class for view support, but is not required to return a response
    //note that the Controller class MUST be public, for it is instantiated by the app as a service, when the route is matched to handle the custom logic over the request
    //using this attribute tag below eliminates the Controller naming convention check in the class itself, allowing any custom name for the controller
    [Controller] 
    public class HomeController : Controller
    {
        //the Controller class provides additional useful methods for controllers, like Content() method to create ContentResult, model bindings, validations etc, otherwise it is not a required inheritance

        //the [] tag below is called an Attribute,
        //it creates the correlation of the named Route validation to the action method under it.
        //multiple routes can be corelated to the same custom action method, here url-route and url-route-same-action both route the request to customAction()
        //string validations, custom route constraint classes, use of {variables} and :type modifiers like regex all work within Route()
        //same as a endpoints.Map("", ()=>{}) call within UseEndpoints, but since the Controller is injected to the app builder as a service
        //the MapControllers call on the server will find and automatically map these attribute tagged routes to the related custom logic
        [Route("url-route")]
        [Route("url-route-same-action")]
        public string customAction()
        {
            //the name of the custom action can be anything, not correlated to the route, but preferably relatable
            //any type can be returned, by default IActionResult or void
            return "Hello from HomeController.customAction() at root/url-route";
        }

        //IActionResult is parent of all Result type classes shown further down. so being that calling Json() returns a JsonResult class object, which is a child of IActionResult, it is acceptable
        [Route("iaction-example")]
        public IActionResult ActionExample()
        {
            return Json("IActionResult return example");
        }


        //the action method associated with root or home is called Index
        //the return type here is ContentResult type, where a new ContentResult class is instantiated and returned content being anything, even an instance of a class, a string int, an html page etc
        [Route("home")]
        [Route("/")]
        public ContentResult Index()
        {
            //ContentResult is not sent to the browser directly, this object is used by ASPNET to generate the actual response sent back to the client
            //return new ContentResult()
            //{
            //    //the Content is the actual content body of the response that can be anything, even an instance of a class, a string int, an html page, javascript code etc
            //    Content = "Home Page",
            //    //the ContentType will be aded as a header to response to tell the client what type of data is in the response, such as text/plain, text/html, text/json etc
            //    ContentType = "text/plain"
            //};

            //this is a shorthand way of doing the same as the comment above, Content() is provided by Controller parent class whoese methods can be called by the child method HomeController in this case, note that the return type is still ContentResult, which Content() returns internally. 
            return Content(
                "<h1>Home Page</h1>" +
                "<p>Welcome to the website</p>", "text/html");
        }

        //this example shows the use of Json data in the response
        [Route("about")]
        [Route("about-us")]
        public JsonResult AboutPage()
        {
            //this AboutData is an example data Model
            //the point of a data model is that it is a specific format of data that can be preset, instantiated and converted into a transmittable data format in the response
            AboutData model = new AboutData() 
            { 
                PhoneNumber = 0222333,
                ContactName = "responsible party",
                RequestGuid = Guid.NewGuid()
            };

            //any object can be passed and converted into json at runtime
            //this will also set the ContentType to application/json automatically
            //typically this data is not returned directly like this, its instead handled by client side methods that receive the Json data to decode it into client side data instead of simply displayed on the page
            return new JsonResult(model);

            //also shorthand method returns a new JsonResult()
            //return Json(model);
        }

        //this route shows a route constraint that requires that phone be an int and 10 digits, simulating as if the hours for a specific hotline would be returned
        [Route("contact-us/hours/{phone:int:regex(^\\d{{10}}$)}")]
        public string PhoneHours()
        {
            return "hours of operation: 0:00-0:00";
        }

        [Route("contact-us")]
        public string ContactPage()
        {
            return "Contact Page";
        }

        //this example shows FileResult return type, where what is being returned is a file download directly, examples such as pdfs, zip files, exe files etc
        //note the return type needs to match the method used, three examples below
        [Route("terms-and-conditions/virtual")]
        public VirtualFileResult DownloadTermsVirtual()
        {
            //note that the relative path means, from the starting point of the static files folder, where is the file
            //VirtualFileResult("relativePath", "ContentType");
            return new VirtualFileResult("/sample.pdf", "application/pdf");

            //shorthand syntax provided by Controller parent class
            //return File("relativePath", "ContentType");
        }

        [Route("terms-and-conditions/physical")]
        public PhysicalFileResult DownloadTermsPhysical()
        {

            //PHysicalFileResult requires the full absolute path per the local machine running the server app. not ideal
            //PhysicalFileResult("absolutePath", "ContentType");
            return new PhysicalFileResult("E:\\source\\repos\\ASPCourseSection6\\ControllerExample\\wwwroot\\sample.pdf", "application/pdf");

            //shorthand syntax provided by Controller parent class
            //return PhysicalFile("absolutePath", "ContentType");
        }

        [Route("terms-and-conditions")]
        public FileContentResult DownloadTermsFileContent()
        {
            //a byte array is a format to store files in memory, that can be stored into databases, rregular data types and files can be encoded into byte arrays for easy transport or storage 
            //it may be useful to store a file as a byte array in database, receive it to server and append directly to response, offloading the processing work of converting the byte_array into the actual file to the client machine, or streaming large files as byte arrays to assemble clientside

            byte[] bytes = System.IO.File.ReadAllBytes("E:\\source\\repos\\ASPCourseSection6\\ControllerExample\\wwwroot\\sample.pdf");
            return new FileContentResult(bytes, "application/pdf");

            //shorthand syntax provided by Controller parent class
            //turn File(bytes, "ContentType");

            //note that ReadAllBytes() returns a byte array, which is then passed to the File which is then processing into the file returned to the client
            //return new File(File.ReadAllBytes("absolutePath"), "ContentType");

            //other examples like filestream can be used as well
        }

    }
}
