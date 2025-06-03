/* when a client sends the http GET request, during basic ddevelopment it goes to kestrel, the application default server, which feeds the request to the custom
 * application code below. a reverse proxy server can be simulated during development as well.
 * 
 * however during production a reverse proxy server will be used like apache or nginx or docker, because of important features like
 * load balancing, url rewriting, authentication handling, caching, decryption of SSL certificates and more, the reverse proxy server or 'container' 
 * handles the initial arriving http GET request, proessess hands it to kestrel who finally hands it to the application code, and the response goes back to client
 * in reverse order, from application code to kestrel to reverse proxy container server to output http response, which is then read and displayed on client software.
 */

using Microsoft.Extensions.Primitives;

internal class Program
{
    //in C#9 main method is not needed, but still implemented here for convention sake.
    private static void Main(string[] args)
    {
            // builder loads the:
            //config = connection strings, app settings,
            //environment = api urls and server addresses,
            //services are predefined and user defined services
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            
            //extra configurations under builder.services, builder.Environment

            //by calling build on builder, we get the webapp that can be configured
            WebApplication webApp = builder.Build();
            
            //here a route is is defined in the webapp, to send the response return on / url path GET request
            //webApp.MapGet("/1337", () => "msg");

            //Run will execute some logic code on the incoming request.
            //the supplied parameter is the HTTP request context information, that contains
            //various fields that can be set or modified conditionally before returning the response
            //also for basic security verificaitions of incoming request
            webApp.Run(async (HttpContext context) => 
            {
                //a GET request will not contain a request body, since it is simply requesting content, so it provides request line at the top and headers
                //a POST request will have a body of content after headers, as the point of post is to provide content client to server
                //and the HttpRequest object is a property of context, has various fields like path, which shows the URL path of the incoming request
                string requestUrlPath = context.Request.Path;

                //the method type identifies if the request is
                //GET, get data
                //Post, send entity object data to server, usually headed to database
                //Put, full update of an entity
                //Patch, partial update of an entity
                //Delete, request deletion of an entity
                //and others...
                string requestMethodType = context.Request.Method;

                //a query string can be concatenated at the end of url path by the client sending the request, based on information gathered on the front end.
                //the query looks like ?queryKey=queryValue
                //this is useful to make more specific requests to the same url path, for example ?id=1 or ?id=2 could both attached as query
                //to path website.com/content, which may read the query and respond with different content
                //query string goes to request body on POST type requests
                //so adding query to end of url ?id=1&page=2 will evaluate to true here, in which case id becomes "1"
                string id = context.Request.Query.ContainsKey("id") == true ? context.Request.Query["id"] : "";

                //both request and response have sets of headers headers are important to qualfy requests as valid and request specific information
                //client and server must also agree on data format, ie. if server requires json info in request, client must provide data in json
                //authentication tokens, session keys, cookies etc can be provided in headers
                //a header is handled as a key value pair
                //the key is tipically camelcase with uppercase first letter
                //the value has no restrictions, including spaces, alphanumeric, symbols
                //https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers <- list of all HTTP headers
                //some notable headers are Content-Type, to stipulate type of data relevant for POST requests, to tell server that data is application/json or text/html for example, Content-Length tells server how many bytes is in request body, Accept-Language requests content in specific language from server for localization, Date current datetime, Host indicates server domain name such as www.website.com, User-Agent indicates Browser or client details/version important for providing content adequate for a specific client version, Cookie contains cookies being sent to the server

                //custom headers can be added, there are some defaults and many presets that do specific things, like Set-Cookie will save cookies at the client,
                //Cache-Control sets time that the response can be cached locally to avoid sending repeat requests within a time set like 60 seconds
                //secure header control is an important safety mechanism, such as Access-Control-Allow-Origin
                //Location header used to redirect, useful for redirecting one request into calling another route to the server
                context.Response.Headers["CustomHeaderKey"] = "customHeaderValue";

                //an HTTP default response header collection has some defaults, which can be edited.
                context.Response.Headers["Server"] = "customServerValue";
                context.Response.Headers["X-Powered-By"] = "theDev";

                //the Content-Type header has various options to indicate what type of data is being sent in the response to the client
                //things like text/plain, text/html json, xml etc, to help the client process the response correctly
                context.Response.Headers["Content-Type"] = "text/html";

                //statuscodes indicate various things to client, 404 is a path undefined at server, 101 is protocol change HTTP/HTTPS, etc.
                //supplies correct header ok request = 200

                if (requestUrlPath == "/" && requestMethodType == "GET")
                {
                    //here the custom authenticationKey header is checked for demonstration
                    //if the key is not present, stauscode signals a bad request
                    if (context.Request.Headers.ContainsKey("AuthenticationKey"))
                    {
                        context.Response.StatusCode = 400;
                        if (context.Request.Headers["AuthenticationKey"].ToString() == "1")
                        {
                            context.Response.StatusCode = 200;
                        }
                    }

                }
                else if (requestMethodType == "POST")
                {
                    //the streamreader reads the Stream object that is the request body
                    //and assigns it to a string format, which is then parsed into a query dictionary of key value pairs
                    StreamReader reader = new StreamReader(context.Request.Body);
                    string body = await reader.ReadToEndAsync();
                    Dictionary<string, StringValues> queryDictionary = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(body);

                    if (queryDictionary.ContainsKey("firstName"))
                    {
                        string firstName = queryDictionary["firstName"][0];
                        await context.Response.WriteAsync($"<h1>hello {firstName}, the server sends this response.</h1>");
                    }
                }

                await context.Response.WriteAsync($"<h1>you've accessed '{requestUrlPath}' url path.</h1>");
                await context.Response.WriteAsync($"<p>requestMethod: {requestMethodType}, responseCode: {context.Response.StatusCode}. queryId: {id}</p>");
                if (context.Request.Headers.ContainsKey("User-Agent"))
                {
                    await context.Response.WriteAsync($"<p>userAgent: {context.Request.Headers["User-Agent"]}</p>");
                }
                if (context.Request.Headers.ContainsKey("AuthenticationKey"))
                {
                    string authString = context.Request.Headers["AuthenticationKey"];
                    await context.Response.WriteAsync($"<p>authString: {authString}</p>");
                }
            });

            //server will not start until Run is called,
            //all config above will be available at once once the application runs
            webApp.Run();
    }
}
