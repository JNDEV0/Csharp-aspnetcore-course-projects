using Microsoft.AspNetCore.Mvc;
using ModelValidationExample.Models;
using ModelValidationExample.Models.Binders;

namespace ModelValidationExample.Controllers
{
    public class HomeController : Controller
    {
        //this example below showcases the [Bind] attribute assigned to the Person model binding target
        //the names of the key value pairs passed as parameters, either in hardcoded string or using nameOf, will define which fields will be correlated to the instantiated Person model from incoming request data.
        //even if other fields and values are given, only named [Bind()] fields will be assigned, the rest of the class fields of the isntance would be null or default values as assigned internally
        //this can be useful when the request context contains many other fields and properties perhaps for multiple purposes and classes, using [Bind] to choose the target properties overposting is avoided by allocating just enough memory to achieve a given goal
        //the same Person.cs Model could be used along with [Bind], to different routes that that would create the Person.cs instance each time with only the relevant fields, such as checking the name and dob, or just the password and confirmPassword, etc
        [Route("register/bound-example")]
        public IActionResult BoundExample([Bind(nameof(Person.Name),nameof(Person.DateOfBirth))]Person person, [FromHeader(Name = "keyName")] string headerRetrievedValue)
        {
            //any fields that are not specifically named within Bind, will be null, and any fields WITHIN the Person.cs class that have the [BindNever] attribute, will also be null

            //also shown here are examples of how to retrieve data from Headers, either custom Headers or default request headers, like User-Agent to identify what browser the request is coming from, using [FromHeader] tag and also inside any actionMethod:
            //ControllerContext.HttpContext.Request.Headers["keyName"]
            
            return View(); //example placeholder
        }

        //the [FromBody] attribute stipulates the values must be retrieved from the request body, so if values are sent in form-data, route or query data, none of those values will end up in the instantiated Person in this actionMethod this restriction can ensure data is being sent in the correct way
        //without stipulating [FromBody], even with actual data in the request body, the app will NOT parse the data into the instance of the model class. so [FromBody] is required if KVPs are being sent in the request body, JSON format by default 
        //the [ModelBinder] tag is used to stipulate a custom "model binder", that is, a class where logic is implemented to handle incoming request data into memory variables, correlated to the given data model target, so here Person is the target object whos properties will be filled from the incoming request, however since there is a custom modelbinder PersonModelBinder's rules will be applied, which means that firstName and lastName could be combined into Person.Name, or any other custom validation/processing with customized control of what happens to incoming request data as it is assigned into the runtime data model target
        [Route("register/from-body-example")]
        public IActionResult FromBodyExample([FromBody] Person person)
        {
            //[ModelBinder(BinderType = typeof(PersonModelBinder))]
            //note that the above attribute was not assigned to the Person person parameter, because the custom PersonModelBinder has already been assigned as default model binder by PersonBinderProvider to the app builder.
            //otherwise the attribute above would be needed after [FromBody]
            return View(); //example placeholder
        }

        //the model binding validation occurs after form data, route data, query string URL, body data and finally model binding that processes the data from any of the above fields, into the class object passed as the parameter
        [Route("register")]
        public IActionResult Index(Person person)
        {
            //using model binding validation the verification that woould be either required in this codeblock or implemented in the route constraints, custom route constraint classes, or parameter validations, can instead be performed in the target model class, so instead of example:
            //if (string.IsNullOrEmpty(person.Name))
            //that requirement and validation takes place in Person class code

            //if any of the Values throw internal errors for validation, being missing and [Required], IsValid will be false
            if (ModelState.IsValid is not true)
            {
                //here a foreach loop to iterate each value's error collection, adding to list 
                //List<string> errorList = new List<string>();
                //foreach(var value in ModelState.Values)
                //{
                //    foreach(var error in value.Errors)
                //    {
                //        errorList.Add(error.ErrorMessage);
                //    }
                //}

                //does the same as above
                List<char> errorList = ModelState.Values.SelectMany(value => value.Errors).SelectMany(error => error.ErrorMessage).ToList();


                //each value passed in the request, will have been parsed out of the request into the target Model class in this case Person.cs, which will apply its validation rules and aggregate errors into the ModelState, here they are being concatenating into one string and returning in BadRequest() response
                string errors = string.Join("\n", errorList);
                return BadRequest(errors);
            }

            return Content(person.ToString());
        }
    }
}
