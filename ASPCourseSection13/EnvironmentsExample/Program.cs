/*
    Setting Environment Variables in Terminal Process execution command
    the command varies depending on terminal type, powershel, command land, etc
    examples:
    $Env:ASPNETCORE_ENVIRONMENT="EnvironmentName" / set ASPNETCORE_ENVIRONMENT="EnvironmentName"
    the above sets the envionment, then the following command inside the project folder can run different enironments
    without using the launchsettings profile
    dotnet run --no-launch-profile

  IMPORTANT! development
  User Secrets are only relevant in development environment, production on the cloud, there are various "Key vault" type solutions, Azure, AWS, etc
  sensitive information should NOT be stored directly in the appsettings.x.json files all would get exposed if the
  source code were to be uploaded or shared. a solution is USER SECRETS which is loaded LAST to the application.
  right click the project solution and choose manage user secrets to see the secrets.json file, use these commands
  in powershell to store sensitive KVPs to local machine's secrets.json:
  dotnet user-secrets init
  dotnet user-secrets set "key" "value"
  dotnet user-secrets list
  the secrets.json file is stored in AppData local machine and appear in Configuration, as well as IOption type injection
  as done in HomeController.cs, Program.cs etc

    IMPORTANT! production
  in production environment variables are best not stored in appsettings.x.json files, because they would be visible if project is shared
  a good solution is using Terminal shell Environment variable storage, to enter secure KVPs in the same terminal that
  executes the app, causing the KVPs to be accessible in the same terminal window but not other terminals, even in same machine
  this allows secure KVPs to be stored outside the app and still accessible without having to change the code setup shown here
  
  example terminal commands:
  $Env:ParentKey__ChildKey="value"
  $Env:ParentKey__ChildKey2="value2"
  dotnet run --no-launch-profile
  this results in ParentKey object having Childkey and Childkey2 properties each with their own value,
  stored as environment variables of the terminal 
 */

//see Controllers/HomeController.cs

using EnvironmentsExample.AppOptions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

//to be able to retrieve Options Model type classes, that are composed of data retrieved from appsettings.json
//using the master key name, and the nested fields are loaded into the target model instance
//so the target instance model is passed as generic, and parameter is the target section key in Configuration collection
//which is populated based on KVPs retrieved from appsettings.json
//the dependencyInjection as parameter to constructor of controller or service will be of IOptions<targetOptionsModel> type
builder.Services.Configure<WeatherApiOptions>(builder.Configuration.GetSection("weatherAPI"));

//by default the builder.Configuration  will have data from appsettings.json, appsettings.x.json,
//user secrets in development mode, environment variables in terminal, and last to all default values
//is the custom Configuration, which can define a custom file to retrieve values from
//THIS OPTION OVERRIDES values retrieved from other sources
builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    //by calling ConfigureAppConfiguration, this lambda expression is called
    //then the target json file is passed into the app.Configuration collection
    //optional true so no error if missing json file
    config.AddJsonFile("MyOwnConfig.json", optional: true, reloadOnChange: true);
}); //this option of specifying a custom config source is rare in production

var app = builder.Build();

//simple example of showcasing use of current environment setting
//to enable or not the developer exception page view output in case of exception
//see Solution/Project/Properties.launchSettings.json for environment setting for each selected launch profile
//which includes kestrel, IIS express, https, WSL and any additional custom profiles, all have a:
//"ASPNETCORE_ENVIRONMENT": "currentEnvironmentName"
//setting that defines currest environment setting, this can be accessed in controllers as well to change runtime behaviour
//if (app.Environment.IsDevelopment() ||
//    app.Environment.IsStaging() ||
//    app.Environment.IsEnvironment("customName"))
//{
//    app.UseDeveloperExceptionPage();
//}

app.UseStaticFiles();
app.UseRouting();

//storing KeyValuePairs are useful for environments
//because they offer several ways to store variables
//that may change depending on environment, sinple  example:
//the Dev environment connects to test API rather
//than the live production API, also uses a test database
//the Release environment points the program to the live additional APIs it will use, and points to
//the real production database

//appsettings.json
//

//the app has access to appsettings.json
//where it can access stored Key Value Pairs
//using the Configuration property
//app.UseEndpoints(endpoints =>
//{
//    //three ways to access Environment KVPS from appsettings.json, where they can be stored and retried by key, so not entirely environment specific but can be called conditionally on if (Environment) validation
//    endpoints.Map("/", async context =>
//    {
//        //note that the key name is NOT case sensitive
//        await context.Response.WriteAsync(app.Configuration["myKeY"]);

//        //here the GetValue<>() method is passed the type of expected value from the key, given that all kVPs are stored as string in target json file appsettings.json, this can be useful to get the expected type from storage
//        await context.Response.WriteAsync(app.Configuration.GetValue<string>("MyKey"));

//        //here a default fallback value is set in case the key is missing
//        await context.Response.WriteAsync(app.Configuration.GetValue<string>("MissingKey", "defaultValueIfKvpDoesNotExist"));
//    });
//});

app.MapControllers();
app.Run();
