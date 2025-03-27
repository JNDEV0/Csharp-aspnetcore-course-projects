/*
 in this example first the packages are imported, for EFCore.sql and EFCore.tools, a controller is created to receive the api request, a city data model class is created and a database context class to insert the data model into the database, the connection string is added along with the DbContext to the builder.Services, note the connection string is in appsettings.json after being retrieved from the database provider, in this example using sql server object explorer and viewing properties of the database.

    once above steps are completed, using Add-Migration and Update-Database should create the table/rows for the city data model in the target CitiesDb of database provider


notes on Swashbuckle/swagger and openapi packages, note the dependencies on Example.WebAPI, swashbuckle.aspnetcore is a framework that integrates swagger into asp.net c#, swagger is a set of tools used to generate client UI to document and test api services, and OpenAPI is a specification for request/responses with APIs
note swagger has 4 operations in this file, mapping endpoints, adding documentation, enabling swagger route, enabling UI 

note that enabling swagger will not open it automatically on executing the solution, launchSettings.json need to point to the new swagger api route as well:
for example if default route is "applicationUrl": https://localhost:7221/ "launchUrl": "swagger" will auto open the api route to the swagger UI

CORS notes:
Cross Origin Resource Sharing
client internet browsers by default prevent cross-domain or cross-origin responses to be read. this means that server1.com sends a view to clientbrowser, if the user or view content attempts to send a request to server2.com FROM 
 */

using Asp.Versioning;
using CitiesManager.Core.Identity;
using CitiesManager.Core.ServiceContracts;
using CitiesManager.Core.Services;
using CitiesManager.Infra;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

//note using AddControllers instead of AddControllersWithViews because WebAPI does not respond with views, only data
builder.Services.AddControllers(options =>
{
    //the filters set below are stipulating a specific request/response body data type, usually could be plain/text application/json and so on, there are several defaults which would be accepted conditionally depending on what the request sends with Accepts: header, this MIGHT be an issue or not, better to stipulate only the format used if the API only supports one anyways, NOT stipulating this allows more flexibility in request/response types, and setting it in this AddControllers options sets it globally for all post,put,get etc requests and responses, but can still be overriden if needed.
    //viewing the swagger.json file can be used to visualize the OpenAPI specification, setting the global filter below will limit the request/response type to only application/json by default, but can be overriden for specific actionmethods using [Consumes] and [Produces] tags, see CitiesController.cs

    //ConsumesAttribute stipulate the request "type" globally
    options.Filters.Add(new ConsumesAttribute("application/json"));

    //set the response "type" globally
    options.Filters.Add(new ProducesAttribute("application/json"));

    //adding a global filter policy that has RequireAuthenticatedUser enabled, this effectively enables Jwt validation as configured later in this file
    //for all incoming requests, EXCEPT controllers tagged [AllowAnonymous], without this global filter [Authorize] tag is needed to enable the jwt validation
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(policy));
}).AddXmlSerializerFormatters(); //enables functionality to generate api.xml file from ///<summary> comments on actionMethods, for SwaggerUI

// Add DI services to the IoC containers.

//look into enabling quick properly and certificate setup, for now commented out, without setting up certificate http3 connections are rejected, http2/1.1 work normal
//using Quic enables faster transport protocol than http1 or http2, but requires a valid certificate issued by certAuthority or self signed, and causes some security popups when executing
//X509Certificate2 cert = CertificateLoader.LoadFromStoreCert("localhost", StoreName.My.ToString(), StoreLocation.CurrentUser, allowInvalid: false);
//builder.WebHost
//    .UseKestrel()
//    .UseQuic()
//    .ConfigureKestrel((context, options) =>
//    {
//        options.Listen(IPAddress.IPv6Loopback, port: 7822, listenOptions =>
//        {
//            listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;

//            //to actually use http3, a certificate is needed, otherwise will default all requests to http2
//            listenOptions.UseHttps(httpsOptions =>
//            {
//                //httpsOptions.ServerCertificate = cert;
//            });
//        });
//    });

//swagger read/map endpoints
//Adding enpoint explorer enables Swagger to read the routedto/endpoint actionmethod's metadata http method, url, attributes
builder.Services.AddEndpointsApiExplorer();

//1st for versioning, enable swaggerDoc for each version, the title and version number are included in the swagger.json generated doc, later the api versioning and apiexplorer settings enable the incoming request to be parsed from the URL segment, thought it can be from query or header as well
//generates documentation on all routes
//enables swagger generated documentation client UI of mapped api endpoints/routes
builder.Services.AddSwaggerGen(options =>
{
    //XML comments for each actionmethod in controllers can be written and extracted to api.xml right click solution>properties>Build>XMLDocumentationPath on solution build generates the output .xml file that swagger then can map the comments to each api endpoint in the UI display, useful for quick reference of each endpoint on swagger ui page options need to be enabled at builder.Services.AddSwaggerGen() see Program.cs
    //once enabled as described above pointing to the output .xml file, Swagger will display IF the endpoint has /// xml <summary> comments at controller
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "api.xml"));

    //swager documentation needs to be added for each "version" of an api in case versioning is enabled,
    //here simply manually hardcoded, there are more dynamic ways of resolving this step
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Cities Web Api", Version = "1.0" });
    options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Cities Web Api", Version = "2.0" });
});


//adding the CustomJwtService so builder.services as transient(instantiated while used), so it can be injected to controllers that handle Jwt tasks
builder.Services.AddTransient<IJwtService, JwtService>();

//adding CORS header policy modifications
//the DEFAULT DEFAULT (lol) cors policy is a client side rule that will prevent client from reading a response received from a cross-domain request,
//ie: website1.com view response has a link to send PUT to website2.com, user clicks the link, website1.com client receives response with (donotread) type header because the request sent to website2.com did not originate from the same domain that is website2.com, so the client by default does not read the response in this case to prevent a hack attack vector, so in truth this is quite safe as is,
//BUT cors needs to be enabled at some point in a production scenario since custom headers are used in HTTP type requests, ie authentication, sessionKey, etc
builder.Services.AddCors(setup =>
{
    //the hereby stipulated as default custom policy
    setup.AddDefaultPolicy(policyBuilder =>
    {
        //CorsPolicyBuilder class, note that the Withx() methods return CorsPolicyBuilder as well,
        //so they can pass on the updated object to the next method call
        //origins is an array of domains as: protocol://domain:port,
        //the "origins" are string values for the remote client above combination sender client expected in the incoming request,
        //if sent from another domain, fails the cors policy check response will not be read by browser
        //not safe! the origin value string is being retrieved from appsettings.json env variables, no validation is done to ensure the KVP even exists

        //the WithHeaders() should contain the named set of headers that are expected with the incoming request,
        //include default headers or make sure client app does not send them, the point is headers are used extensively
        //additional custom headers are added, headers are evaluated as a whole package, either all present in request or fail
        //this is why later there are AddPolicy() where addicional policies can be added for specific controllers or actionmethod/endpoints
        //so a broader/default cors policy for all incoming request, additional overriding custom policies for specified endpoints

        //WithMethods() stipulates the type of request method operation that can be requested,
        //ie: an API that ONLY responds to GET requests would have only GET as param passed to WithMethods()

        //on a last note: internal app routes such as api/ or cities/ etc or even data model validation of request are not relevant to CORS validation
        policyBuilder
        //.WithOrigins(builder.Configuration.GetRequiredSection("AllowedOrigins2").Value.ToString())
        //.WithHeaders(["defaultHeaders", "customHeadersExampleExpectedAuthToken", "..."])
        .WithMethods(["GET", "POST", "DELETE"]); //.WithExposedHeaders etc
    });

    //AddPolicy differs fromAddDefaultPOlicy in that it is NOT used by ANY controller or route by default
    //policies beyond the default need a customPolicyName and the controller or actionmethod need to have the [Cors("customPolicyName")] annotation
    //when the tagged controller or actionmethod is then accessed, the non-default policy will override and apply instead of the default
    //see Controllers.v2.CitiesController.cs for [tag] annotation
    //this is useful to further narrow down request CORS validaitons (origin, headers and request method) checks, beyond the global request default
    setup.AddPolicy("customPolicyName", policyBuilder =>
    {
        //policyBuilder
        //.WithOrigins(builder.Configuration.GetRequiredSection("AllowedOrigins").Value.ToString())
        //.WithHeaders(["defaultHeaders", "customHeadersExampleExpectedAuthToken", "..."]); 
        //.WithMethods,.WithExposedHeaders etc
    });
});

//note that this code may appear on older implementations, but does not work with the new Asp.Versioning.Mvc package
//instead the ApiExplorer is added after the AddApiVersioning call
//builder.Services.AddVersionedApiExplorer(options =>
//{
//    options.GroupNameFormat = "'v'VVV";
//    options.SubstituteApiVersionInUrl = true;
//});

//Api versioning makes use of the structure present in the Controllers folder, where v1 and v2 correspond to two versions of the same controller class, they are in separate namespaces and folders per version, and controllers need to have the [ApiVersion("x.y")] tag, install packages: Asp.Versioning.Mvc and .ApiExplorer 
builder.Services.AddApiVersioning(config =>
{
    //by default simply enabling versioning does not enable a way to determine which version to use
    //here the reader is instructed to retrieve the version number from the incoming request URL,
    //so request URI route: localhost:7211/api/v2/cities would route to Controllers>v2>CitiesController.cs

    //there are several options for reading the api version, QueryStringApiVersionReader, Header-, UrlSegment-
    config.ApiVersionReader = new UrlSegmentApiVersionReader();

    //setting the Default and setting AssumeDefault to true enables incoming requests to ommit the version number
    //instead of localhost:7211/api/cities?version=1 now localhost:7211/api/cities points to api.v1 by default IF no value is given
    //this setting applies regardless of wether version is being looked for at url segment or header, of none is given
    //IDE might throw false positive errors on this class? some constructors expect DateTime instead of version int
    config.DefaultApiVersion = new Asp.Versioning.ApiVersion(1,0);
    config.AssumeDefaultVersionWhenUnspecified = true;

    //looking for a version param in the req headers "custom-api-version-header": 1
    //config.ApiVersionReader = new HeaderApiVersionReader("custom-api-version-header");

}).AddApiExplorer(options =>
{
    //adding the ApiExplorer is important for Swagger to distinguish the formatting of the incoming version, via groupnameformat
    //the 'v' char taken as literal text was stipulated in the URL segment, uppercase V for each possible char, 1.0 would require VVV, 2.323 would be VVVV
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true; //relevant to UrlSegment type, if version was in a req header then this is not needed
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

//AddIdentity is called on builder.Services, receives customUser and customRole, use options param to configure the Password.Require properties, important to remember password security is extremely serious, production password handling should encrypted at every step of entry, transport processing and storage wherever possible. this project does not HTTPS setup so the password is received in plaintext from the client request, this is for example brevity. the system uses this "username" and "password" terminology, but could easily hold some alternative, such as a walletPublicAddress and a signedTransactionId in relation to crypto wallets, so the server could validate a signed message from the user's wallet/private key, on a blockchain, instead of storing a user's password, even encrypted. on the other hand a well encrypted and handled password is safe as well, such sending a secretString server->client and client encrypts password with secret when sending in transit, server decrypts to validate against the stored passwordEncryptedString, that way the actual password is perhaps only ever handled on account registration, an example.
//adding efcore store takes <T> of CustomDbContext that implementes the IdentityDbContext, then the token provider is chosen, then adding User and Role stores, where AddUserStore receives a UserStore<t1,t2,t3> with stipulated CustomUser, CustomRole assigned to said user, CustomDbContext that will handle user's auth, Id type which here is guid. all these calls are before the app is built and the middleware stack that handles requests is called later at app.UseAuthentication()
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    //here password must min 6 characters by default, an uppercase and lowercase, and at least 1 number
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.User.RequireUniqueEmail = true;
    //various other options relating to Identity, like rules for Usernames etc
}).AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddUserStore<UserStore<AppUser, AppRole, AppDbContext, Guid>>()
.AddRoleStore<RoleStore<AppRole, AppDbContext, Guid>>();

//AddAuthentication configures the auth middleware to use Jwt token validation
//EVEN THOUGH jwt is configured here, added to Services and later added to the app.middleware stack below
//it will ONLY take effect for controllers with the [Authorize] annotation strangely enough
//UNLESS this is inverted, and a global policy filter is added for ALL controllers EXCEPT controllers that have [AllowAnonymous]
//which makes sense if building an app that has lots of authenticated activities
//see builder.Services.AddControllers() call towards top of this file, where the global filter policy is applied to make all requests requireAuthUser by default
builder.Services.AddAuthentication(options =>
{
    //default will check Jwt validation first, if this fails it will fallback to the DefaultChallenge
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

    //DefaultChallengeScheme will trigger if JWt fails, for example made up token/invalid/expired
    //by default returns 401 unauthorized response, and the clientside would handle redirect/refresh attempt 
    //point is invalid token has no fallback, if validation fails, it fails the challenge as well
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

    //setting the DefaultChallenge to using cookies requires configuring cooking authentication not done here, but is an alternative
    //options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddJwtBearer(options => //configuring the jwt token validation that has been set as default above
    {
        //if any of the validations fail, the DefaultChallengeScheme will trigger, this happens when JWT is invalid, expired or fake
        //will require login or refresh depending on situation
        options.TokenValidationParameters = new
        TokenValidationParameters()
        {
            //checking origin and destination are from/to intended domains
            ValidIssuer = builder.Configuration["Jwt:Issuer"], //setting the expected server domain:port
            ValidAudience = builder.Configuration["Jwt:Audience"], //setting the expected client domain:port 
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
            ValidateIssuer = true, //enable check Issuer/server domain:port of incoming jwt
            ValidateAudience = true, //enable check "Audience"/client domain:port of incoming JWt

            //checking if not expires and expected signature/hashedPayload of token
            //this part is the most important to ensure 1token == 1validSession, if details are different, token is invalid
            //note the IssuerSigninKey is a SymmetricSecurityKey(ByteArray secretKey) the server retrieves secret security key,
            //which is then used by ValidateIssuerSigningKey to check that the token has the expected content
            //as an example imagine user has logged in received token and sent in an authed request:
            //client has not decompiled or used the token in any expected valid way, if tampered/altered the forgedToken is set to fail here
            //valid client have authSession unique tokens for their own postAuth recognition, sent in with valid requests that pass these checks
            ValidateLifetime = true, //enable check token not expired
            ValidateIssuerSigningKey = true //enable check that signature which == encoded(hash(payload+secret))
        };
    });

//adding options to Authorization can be useful
//to configure what happens after the Jwt validations
//builder.Services.AddAuthorization(options => { options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicy(authReq,  authScheme)});
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.

//enabling the wwwroot public files folder
app.UseStaticFiles();

//force HTTPS, requires launchsettings config
app.UseHsts();

//routing here is enabled to illustrate that CORS needs to be enabled after app is built and routing/endpoint is determined
app.UseRouting();

//CORS policy modifications need to be added to the request middleware pipeline
app.UseCors();

app.UseHttpsRedirection();

//swagger is a endpoint specification mapper with UI for devs
//note that swagger needs to have middleware enabled after app is built
app.UseSwagger(); //created endpoint/route for swagger.json where OpenAPI specification is stored, info related to actionMethods of controllers
app.UseSwaggerUI(options =>
{
    //adding a "swagger endpoint" route within the swagger generated UI, that corelates with the given url route,
    //note that we have url segment reader for versioning and default to v1 currently
    //each endpoint added here is connected to the SwaggerDoc documentation generated at builder.Services.AddSwaggerGen call where a OpenApiInfo object is passed, a title and a version "name"
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "1.0");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "2.0");
}); //enable the UI to view and test the api using swagger

//the order of middleware is important, UseRouting , UseCors, UseAuthentication, UseAuthorization and MapControllers
//UseAuthentication defaults to cookie authentication, for JWT it needs to have options configured, note this uses the aspnetcore.authentication.jwtbearer package, same package imported to CitiesManager.Entities, the configuration is at builder.Services.AddAuthentication, the call below is the configured middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
