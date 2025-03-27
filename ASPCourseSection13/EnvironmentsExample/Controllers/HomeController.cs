using EnvironmentsExample.AppOptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EnvironmentsExample.Controllers
{
    public class HomeController : Controller
    {
        //at Program.cs the app.Environment is of IHostEnvironment type
        //to access the Environment name witihn a controller it can injected
        //as a parameter to the controller constructor
        private readonly IWebHostEnvironment _webHostEnvironment;

        //IConfiguration is the same type as app.Configuration in Program.cs
        //which is injected to the controller constructor in order to access
        //stored KVPs from appsettings.json
        //this injection can be done with Controllers, Services etc
        private readonly IConfiguration _configurationKVPs;

        //the "options" are KVPs from appsettings.json model bound to a target class of Options type
        //so the fields of the target generic type below will determine what keys are retrieved from the
        //stored master key, configured into the builder.Services.Configure() call in Program.cs
        //essentially dependency injection, as a Service, "options" is just the naming convention
        //for the environment variables retrieved this way, since they are intended to change depending on environment
        private readonly WeatherApiOptions _options;

        public HomeController(IWebHostEnvironment webHostEnvironment, IConfiguration configurationKVPs, IOptions<WeatherApiOptions> optionsModelEnvVariables)
        {
            _webHostEnvironment = webHostEnvironment;
            _configurationKVPs = configurationKVPs;
            _options = optionsModelEnvVariables.Value; //the injected parameter is of IOptions<T> type, the .Value is the target optionsModel
        }

        [Route("/")]
        [Route("IntentionalError")]
        public IActionResult Index()
        {
            //once the homecontroller is instantiated the IWebhostEnvironment offers 
            //these methods, that function same as the app.Environment.Isx() equivalents in Program.cs
            //but now inside the controller
            if (_webHostEnvironment.IsDevelopment() ||
                _webHostEnvironment.IsStaging() ||
                _webHostEnvironment.IsProduction())
            {
                //task examples could be fetching from a different database depending on environment
                //or enabling/disabling logging services
                //...
                ViewBag.EnvironmentName = _webHostEnvironment.EnvironmentName;
            };

            //KVPs are stored in json format within appsettings.json
            //after the IConfiguration is Inversion of Control Dependency Injected into the constructor of the Controller
            //it can be accessed for values within the KVP collection same as app.Configuration
            ViewBag.EnvironmentVariable = _configurationKVPs["MyKey"];

            //the example below uses the .GetValue method instead of accessing the key's value directly as above
            //note that the second parameter is a default value if the key is missing from appsettings.json
            ViewBag.MisingEnvironmentVariable = _configurationKVPs.GetValue("expectedKey", "defaultValueIfKeyIsMissing");

            //GetSection can be called to retrieve a nested key, the named parameter section is the top level key
            //and returns an array of the nested keys inside the target section
            IConfigurationSection jsonSection = _configurationKVPs.GetSection("weatherAPI");

            //the data section can be retrieved as a data model
            //note that the target model class will retrieve ONLY the keys mentioned
            //if a nested section has 10 keys but only 3 are targeted properties
            //the object below will have 3 values, null checking advised
            //Get() loads the values into the new instance and returns the model object
            WeatherApiOptions jsonSectionToOptionsModel = _configurationKVPs.GetSection("weatherAPI").Get<WeatherApiOptions>();

            //another way is to use Bind();
            //Bind loads the values of the target section, to the target options model
            //that MUST already be instantiated, so if the option will only be instantiated once
            //instead of updated Get() may be better
            WeatherApiOptions bindExample = new WeatherApiOptions();
            _configurationKVPs.GetSection("weatherAPI").Bind(bindExample);

            //now the object can be used to read values
            ViewBag.ClientId = jsonSectionToOptionsModel.ClientId;
            ViewBag.ClientSecretKey = jsonSectionToOptionsModel.ClientSecretKey;

            //the two examples below show the use of the Dependency Injected options model from constructor
            //the point is that the options model can have fields for the target nested KVPs that was Configure()'d in builder.Services
            //if a Master KVP has other nested KVPs, only the named fields in options model will be retrieved
            ViewBag.ClientId2 = _options.ClientId;
            ViewBag.ClientSecretKey2 = _options.ClientSecretKey;

            ViewBag.ClientIdFromEnvironmentVariable = jsonSection["ClientId"]; //== _configurationKVPs.GetSection("weatherAPI")["ClientId"]

            //in this example we are simply outputting to view the retrieved data,
            //but typically these settings/options would be used to configure the application for a given environment
            return View(); //see Views/Home/Index.cshtml
        }

        //this IntentionalError route is used to demonstrate how changing the Environment will cause a different
        //output to the view client when this route is accessed, since within Program.cs there is a check for
        //if app.Environment.IsDevelopment() for example, then developer error page will be returned to view,
        //if IsStaging() no developer error page in response
        [Route("IntentionalError")]
        public IActionResult ErrorIndex()
        {
            return View();
        }
    }
}
