using Autofac;
using Autofac.Extensions.DependencyInjection;
using xServiceContracts;
using xServices;

var builder = WebApplication.CreateBuilder(args);

//ASP.NET has built in library Microsoft.Extensions.DependencyInjection which is showcased in most examples of this project
//Autofac is a third party service provider factory, that offers additional builder services functionality
//like additional ServiceLifetime options beyond Transient, Scoped and Singleton
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());


//In dependendy Injection implementation, the service needs to me containerized
//for Inversion of Control(IoC) in this case builder.Services
//is the container for any xServices, which implement xServiceContract interfaces.
//the HomeController is the client, the Service Implementation is the Dependency, the IoC is the Dependency Inversion container
builder.Services.AddControllersWithViews();

//the containerized service is added to the builder.Services Wrapper as ServiceDescriptor
//the point is that builder.Services now knows, that when the ICitiesService is called by HomeController
//CitiesService is the actual target implementation, so it will instantiate CitiesService to handle the request
//this causes HomeController and CitiesService to both depend on ICitiesService, instead of each other directly
//now HomeController needs Dependency Injection to be able to retrieve the actual CitiesService instance through the IoC container
//builder.Services.Add(new ServiceDescriptor(
//    typeof(ICitiesService), //the interface on which controller and service depend, Dependency Inversion Principle
//    typeof(CitiesService), //the actual service class
//    ServiceLifetime.Scoped //transient==instantiated 1per IService instantiation, scoped==1per entire request, singleton==1per app/server execution
//));

//shorthand examples of the above example that shows the longform way of adding Dependency Injection
//these shorthand versions do the same
//builder.Services.AddTransient<IService, Service>();
//builder.Services.AddScoped<IService, Service>();
//builder.Services.AddSingleton<IService, Service>();

//using autofac, to add a dependency injection service to the builder
//the Host.ConfigureContainer is passed the ContainerBuilder generic type from autofac
//and a lambda expression where the target service type is registered, or linked
//to the Dependency Inversion Interface that will be called,
//autofac here is used as a replacement way of applying Dependency Injection into the builder
//equivalent to the above described alternatives that use standard ASP.NET instead
builder.Host.ConfigureContainer<ContainerBuilder>(ContainerBuilder =>
{

    //the InstancePerDependency() call stipulates the ServiceLifetime autofac equivalent,
    //Transient
    //ContainerBuilder.RegisterType<CitiesService>().As<ICitiesService>().InstancePerDependency();

    //Scoped
    ContainerBuilder.RegisterType<CitiesService>().As<ICitiesService>().InstancePerLifetimeScope();

    //Singleton
    //ContainerBuilder.RegisterType<CitiesService>().As<ICitiesService>().SingleInstance();

    //instancePerOwned and InstancePerMatchingLifetimeScope are additional setting types
});

//a side note on nesting service injections into other services,
//while it can be done, Transient or scoped services types are best
//since injecting a transient service into a Singleton will case it to not be disposed of properly
//this issue is called Captive Depency

//in summary, the Controller receives the Service logic instance through/from the IoC/DI Containerized interface
//multiple Services are all added this way, and then can be instantiated through the Controllers that receive the named IService interface as parameters
//on Controller instantiation upon request, using [FromService] tags in actionMethods, and child scopes with using and scopeFactories
//this allows specific control of which controller instantiates which services, no need to instantiate all if only some are used
//and Services can depend and call on and call each other, for example using ICitiesService may need to call IGetCitiesFromDatabaseService for example


var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
