/*
    the httpclient is a way to have the server act as a client, sending html.requests
    to external http routes/apis and getting a html.response back, s

    so that this app server acts as a "http client" same as a browser would by attempting to access the correct URL/URI
    and process the json response server side, and then responds to THIS server, without an external client
    this allows this server application to act as a HTTP client, in the eyes of the target server receiving the request from this one
 */

using HttpClientExample.Models.Options;
using HttpClientExample.ServiceContracts;
using HttpClientExample.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

//adding the httpclient service to builder, this enables IHttpClientFactory
//Dependency Injected parameter to any controller or Service in the constructor
//on instantiation will receive the HttpClientFactory instance through the service IoC container
//see Services/HttpClientService.cs
builder.Services.AddHttpClient();

//adding scoped service to Services collection
//note that this example IS NOT Dependency Injection ith Inversion of Control
//which would require a IHttpClientService to invert the dependency of controller and service
//HttpClientService is instantiated directly in the parameter of the constructor see HomeController.cs
builder.Services.AddScoped<IHttpClientService, HttpClientService>();

//adding IOptions<StockQuote> as a service, so it can be injected into constructors of controllers and services
builder.Services.Configure<StockQuote>(builder.Configuration.GetSection(nameof(StockQuote)));

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
