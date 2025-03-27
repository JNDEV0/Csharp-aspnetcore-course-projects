using ModelValidationExample.Models.Binders.Providers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

//custom binder providers are not commonly used, but to avoid having to use [attribute] tags in actionMethod parameters, by inserting the custom provider at the first position in the ModelBinderProviders collection,
builder.Services.AddControllers(options =>
{
    //whenever a Person type parameter is passed to an actionmethod, the custom model binder named inside the provider will be used instead of standard model binding
    options.ModelBinderProviders.Insert(0, new PersonBinderProvider());
});

//by default, ASP.NET can deserialize application/json Content-Type data from the requestbody, to enable application/xml, this there is this method below to add the formatters to the app builder
//by enabling the XML formatter, if data is sent in xml format in request body it will be able to parse to [FromBody] tagged data model types being passed as actionMethod parameters
//note that the frontend has the responsability of encoding, sending the properly formatted data and stipulating which type is it, the server will detect and deserealize into serverside memory
//builder.Services.AddControllers().AddXmlSerializerFormatters();

var app = builder.Build();

app.UseStaticFiles();

//when a request is received, data is parsed from request into memory, before validation, then the routing identifies what actionMethod to call, and finally Model binding occurs as part of the actionMethod's custom Model's validation, set using Validator attributes or custom Validators, which internally run the low level verification logic
//this way model binding allows organizing the code to a set Data Model with organized internal validation for the expected pattern of Data associated with a valid request, with proper error handling per route
app.UseRouting();
app.MapControllers();

app.Run();
