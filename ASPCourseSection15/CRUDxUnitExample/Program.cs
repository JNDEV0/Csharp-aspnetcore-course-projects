/*
    test driven development seeks to implement what is needed for the test FIRST,
    the functionality follows requirement - implementation follows unit test, unit test first.

    the purpose of this example is to showcase how xUnit tests and Data Transfer Objects are useful
    in the Entities Library there are classes for Country and Person, these are considered Domain classes
    meaning they should not be accessed directly from external sources, so instead DTO wrappers are used
    to transfer the data between requests and the domain classes are only ever instantiated where they are assuredly valid
 */

using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using Services;
using Services.Utils;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

//in fact these services should be scoped to requests, they are singlentons here because
//mockdata is being used to simulate data in local runtime, with a database implementation
//the requests would make scoped calls to the database instead of runtime singleton simulated collections
//note that PersonService has a default true parameter to enable the mockdata usage for this example
//builder.Services.AddSingleton<IMockDataUtil, MockDataUtil>();
builder.Services.AddScoped<IPersonsService, PersonsService>();
builder.Services.AddScoped<ICountriesService, CountriesService>();

//AddDbContext by default is a scoped service, injecting Scoped into Singleton DI services will throw an error, so the CustomServices added above are also Scoped.
//note that the EFCore package needs to be imported to this CRUDxUnitExample project
//as well as where PersonDbContext.cs is defined at Entities project
//adding the dbcontext to the services allows it to be injected to constructors, but it should NOT be injected at the controller directly
//instead inject the DbContext in other scoped DI services where temporary Database access is needed
//calling GetConnectionString by nested name equivalent to ConnectionStrings.DefaultCollection
builder.Services.AddDbContext<PersonsDbContext>(options =>
{
    //see appsettings.json for the connection string, it points to the db server string should not be hardcoded here, it is added at environment variables in appsettings.json and retrieved conditionally per environment  dev points to localdb, staging points to live test db, production points to live production db etc. the connection string stipulates various settings on how the connection should work between the db system and the app server
    //a note on the Integrated Security=True part, it means windows anthentication in the sql server, this option may be changed if the database should use a secret username and password for db access instead
    //for this example, the PersonsDatabase is created manually through view > SQL Server Object Explorer to open default MSSQLLocalDB instalation, right click to create the database and then open its properties, the connection string is a property of the created database
    //in a different scenario, the connection string might come from a MongoDB cluster, or a local SQL server with a GUI, etc above step is only relevant for this example
    //calling GetConnectionString by nested key name to fetching ConnectionStrings.DefaultConnection
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
