using Microsoft.AspNetCore.Mvc;
using xServices;
using xServiceContracts;
using static System.Formats.Asn1.AsnWriter;
using Autofac;

namespace DependencyInjectionExample.Controllers
{
    //controller is like a main street of small businesses, a customer goes into a store front
    //which is built for his Viewing, any business there is interfaced with the backroom Service
    //which defines what that business route/store offers
    public class HomeController : Controller
    {
        //the service is readonly in this example,
        //as there should only be one instance of the service process per request anyways

        //note that in this example, the Services are separated from the program build,
        //instead Services is in a class library, so Services class library needs to be added
        //as a reference to DependencyInjectionExample,
        //then the namespace for the target xService.cs class can be imported
        private readonly ICitiesService _citiesService; //DEPENDENCY INVERTED, RIGHT

        //note that the HomeController is automatically instantiated by the app when this controller is routed to
        //the parameter where ICitiesService is being passed to the constructor is never manually supplied
        //it is provided by the builder.Services Inversion of Control wrapper, that has instantiated
        //target xServices class, CitiesService, to assign to the call for ICitiesService
        //this means when HomeController is called it will receive the instance of the Service named as parameter

        //note the 3 parameters, each one is a call to the app.builder.Services when HomeController is routed to,
        //to pass the containerized CitiesService. each one has been given a GUID to show the effects of ServiceLifetime.Transient
        //parameter that is set when the ServiceDescriptor object is passed to app.builder.Services to define the Dependency Injection
        //Transient and Scope will instantiate the Service class for the time/scope of execution for each request
        //THEREFORE the 3 calls below will have the SAME guid, since all 3 calls from HomeController are from a single request
        //see Views/Home/Index.cshtml
        private readonly ICitiesService citiesService1;
        private readonly ICitiesService citiesService2;
        private readonly ICitiesService citiesService3;
        //a side note on injected services private access level
        //services injected into one class should not be used by another class
        //so they should never be public, because by time classB calls citiesService1 for example
        //it might have already been disposed of. each class that uses injected Services, should inject their own instances instead



        //see the Index() route below, the scope factory allows creating
        //nested scope with the using statement, so a request-wide scoped Service
        //could have nested scoped services that are created and destroyed ASAP
        //for example 1 request for 1 webpage handled by Service1, which has several
        //nested scoped database access services calls
        //private readonly IServiceScopeFactory _scopeFactory;
        
        private readonly ILifetimeScope _scopeFactory;


        public HomeController(ICitiesService serviceInstanceFromDependencyInjectionContainer1, ICitiesService serviceInstanceFromDependencyInjectionContainer2, ICitiesService serviceInstanceFromDependencyInjectionContainer3, ILifetimeScope serviceScopeFactory)
        {
            //note that to use standard ASP.NET scopefactory the last parameter
            //would have to be of type IServiceScopeFactory instead of ILifetimeScope which is from autofac
            //_scopeFactory = serviceScopeFactory;
            _scopeFactory = serviceScopeFactory;

            citiesService1 = serviceInstanceFromDependencyInjectionContainer1;
            citiesService2 = serviceInstanceFromDependencyInjectionContainer2;
            citiesService3 = serviceInstanceFromDependencyInjectionContainer3;
            

            //instantiating the Service relevant to this Controller for example purpose
            //services should be dependency injected, rather than instantiated
            //so dependency needs to be inverted,
            //rather than HomeController depending on CitiesService implementation
            //which can change as development progresses or not even exist immediately
            //this can be seen as HomeController the higher level abstraction that organizes the lower level detailed implementation
            //higher logic cannot depend on lower logic, since lower is more fluid and higher should be solid
            //so the Dependency Inversion Principle is applied via dependency injection
            //which allows CitiesService to depend on HomeController instead
            //the class THAT through Direct Injection, that is, if CitiesService is simply instantiated here:
            //_citiesService = new CitiesService(); DIRECT DEPENDENCY INJECTION, WRONG

            //HomeControler would thus depends on CitiesServices directly,
            //it CAN through Dependency Inversion Principle, depend INSTEAD
            //on an INTERFACE ABSTRACTION, to which CitiesServices should also depend on,
            //thus decoupled, HomeController and CitiesService both depend on ICitiesService
            //which breaks the hardwired dependency, allowing both calling Client/controller class and Service/dependency class to develop independently
            //and the Interface between serves as an endpoint to which the other side can be plugged into later
            _citiesService = serviceInstanceFromDependencyInjectionContainer1; //Inversion of Control received instance, Dependency Injection correct

            //Dependency Injection is just one way to implement Inversion of Control to apply Dependency Inversion Principle
        }

        //using the [FromService] parameter attribute, the same automatic retrieval from app.builder.Services will identify
        //the dependency Service associated with the target IService interface, instantiate it when the Route below is called
        //note that even though _citiesService is receving the service instance when HomeController is constructed
        //the method call below is from the parameter reference may be used when the method has specific service,
        //generally better to DependencyInject services at Controller constructor
        //scoped services should not be used to share data between services within the same request,
        //as they are not threadsafe, should not be handling the same data as other services within the same request
        [Route("/")]
        public IActionResult Index([Microsoft.AspNetCore.Mvc.FromServices] ICitiesService serviceInstanceFromDependencyInjectionContainer)
        {
            //the controller should not be concerned with the internal logic implementation of services
            //the controller just calls the method, handles the output into the models and views
            //which should have their logic separate from controller class as well
            List<string> cities = serviceInstanceFromDependencyInjectionContainer.GetCities();

            //the GUIDs are added to the ViewBag to display in the output view
            //to show the effects of ServiceLifetime.Transient parameter that is set, Scoped Singleton etc
            //when the ServiceDescriptor object is passed to app.builder.Services to define the Dependency Injection type
            //see Views/Home/Index.cshtml
            ViewBag.InstanceId1 = citiesService1.ServiceInstanceId;
            ViewBag.InstanceId2 = citiesService2.ServiceInstanceId;
            ViewBag.InstanceId3 = citiesService3.ServiceInstanceId;

            //this statement below creates a Scope for a containerized service
            //potentially inside other Services or wherever needed,
            //the point is the Service transience is well defined and disposed of after the codeblock/scope
            //WITHIN this using codeblock, the scoped service will have its own instantiation GUID
            //EVEN IF the instantiated service inside the using statement has been injected into the app.builder.Services
            //with a different transience setting, for example if set to Singleton, where the service should have only ONE
            //instanceId GUID for all requests, or set to Transient where it would be a new instance for each IService call etc
            //using (IServiceScope newScope = _scopeFactory.CreateScope())
            using (ILifetimeScope newScope = _scopeFactory.BeginLifetimeScope())
            {
                //the type above changes from IServiceScope in standard ASP.NET calling the CreateScope()
                //to ILifetimeScope calling BeginLifetimeScope() in autofac


                //for the containerized Inversion of Control ACTUAL target service to be instantiated
                //here we call the ServiceProvider.GetRequiredService<IService>() method of the scope object
                //WITHIN A SCOPED USING STATEMENT, should not call GetRequiredService<IService>() outside the limited scope
                //provided by the scope factory's CreateScope()
                //it essentially does the same as Dependency injecting the Interface and service direct to app.builder.Services
                //with the clear difference that here there is no transience setting,
                //since the services in this scope will be disposed of at the end of the clodeblock
                //ICitiesService scopedCitiesService = newScope.ServiceProvider.GetRequiredService<ICitiesService>();

                //autofac equivalent below to statement above
                ICitiesService scopedCitiesService = newScope.Resolve<ICitiesService>();
                //objective of this codeblock could be to fetch data from database service, compile into a List in local memory,
                //this CitiesService is a placeholder example, any service can be Dependency Injected within a child scope this way
                //this service instance is equivalent to the last GUID displayed in the outputView

                //the difference betwee GetRequiredService and GetService is that GetRequiredService will THROW
                //an exception if the target service HAS NOT been added to the app.builder.Services collection
                //and GetService will not throw and instead the value will be null, which should be checked before using
                //both methods should only be called within a defined child scope inside using statements
                //ICitiesService scopedCitiesService = newScope.ServiceProvider.GetService<ICitiesService>();

                //this scope concern involves identifying when the service is disposed of to avoid memory issues
                //EntityFramework resolves this issue, but this is relevant for manual implementations

                //since the line below is within the using statement block
                //the instantiated service above has a GUID, so InstanceId0 within the view will have a guid
                //even after the service instance is disposed of, point is the service only exists for the duration of utility
                //and data retrieved from services can be saved outside the using block
                ViewBag.InstanceId4 = scopedCitiesService.ServiceInstanceId;

                //do not call Dispose within the using statement,
                //that is an automated responsability of the IoC container for the dependency injected service
            } 
            //DI service scope is within the using statement
            //the difference here is that there is no option to determine transience as Transient, Scope, Singleton etc
            //the using statement calls the Dispose() method of newScope
            //which will call the Dispose() method of ALL injected services inside the using codeblock


            //inside the View the Model object will be the cities collection
            //the object of the code in this actionMethod has been to extract the GUIDs of service instances
            //containerized into Inversion of Control containers, either builder.Services or using autofac implementation
            //the result of how many GUIDs match changes depending on the ServiceLifetime setting and child scopes.
            //in Transient setting, which is the project default, the first 4 GUIDs should be the same and,
            //the 5th inside the last box will be different because it is instantiated again inside the child scope
            return View(cities);
        }
    }
}
