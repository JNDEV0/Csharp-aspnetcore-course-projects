using HttpClientExample.ServiceContracts;
using System.Text.Json;

namespace HttpClientExample.Services
{
    //the naming convention here is not a requirement and intentionally generic for the example, the httpclient can be injected
    //into any controller or service class after being added to the builder.Services.  example:
    //a ForecastService uses httpclient to retrieve data from external API
    //a PriceAverageService uses httpclient to check prices on several external APIs
    //given the example a better name for this could be FinnhubStockPriceQuoteService perhaps
    //or further abstracted to work with multiple stock price quote external APIs
    public class HttpClientService : IHttpClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        //the builder.Services.AddHttpClient() call in Program.cs enables the Services to provide the parameter injected below
        //which then stores the reference in this service class
        //the configuration is used to retrieve the API key for finnhub.io from dotnet user-secrets
        //this is done to securely store the data in the local developer machine,
        //in a production scenario there would be a keyvault solution used
        public HttpClientService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;

            /*
             * the API key is stored using Terminal commands from THE PARENT FOLDER of this project's folder:
             * dotnet user-secrets init --project HttpClientExample //initializes a user-secrets project with given name
             * the above command will generate a user-secrets associated key to this project, that will be visible by clicking project on solution explorer ->
             * dotnet user-secrets set "ApiToken" ctat101r01qgsps7t5bgctat101r01qgsps7t5c0 --project HttpClientExample //the key is in quotes, the value no quotes
             * the above stores the KVP in user-secrets in local dev machine under given project name
             * and will create an entry in the secrets.json file of this project, can be viewed right click in solution explorer -> project > manage user secrets
             * note values should not be manually added to user secrets, the above commands need to entered at Terminal
             * the above will result in _configuration being able to access the target KVP, being injected into this Service by the builder.Services of the app
             */
            _configuration = configuration;
        }

        //note that the method is async and returns a Task<> in the definition,
        //but the actual return statement is a Dictionary<string,object>?
        //Task<> represents the asyncronous operation, awaits and returns the target type
        //so HomeController where this method is called, receives a Dictionary<string,object>
        public async Task<Dictionary<string, object>?> GetStockPriceQuote(string stockSymbol)
        {
            Dictionary<string, object>? responseDictionary;
            //IHttpClientFactory provides CreateClient() method which
            //creates a HttpClient class for the operation and disposes of it afterwards
            using (HttpClient httpClient = _httpClientFactory.CreateClient()) //should be used inside using statement
            {
                //the HttpRequestMessage class has various components to populate a valid HttpRequest
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage()
                {
                    //the address is composed of https protocol, to website finnhub.io,
                    //api naming convention to signal api service in this route, version, request type, Query with stock symbol and api token
                    RequestUri = new Uri($"https://finnhub.io/api/v1/quote?symbol={stockSymbol}&token={_configuration["ApiToken"]}"), //URL to send request to
                    //note that the ApiToken is not present anywhere in the sourcecode directly except comments,
                    //it is being retrieved from user-secrets set value, see the constructor of this service
                    Method = HttpMethod.Get, //Post, Update, Delete, etc
                    //headers, body, etc
                };

                //the request is sent to the external route and waits for the response,
                //then stores it in HttpResponseMessage
                //the HttpClient class has several useful methodds like GetAsync, PostAsync, PutAsync, DeleteAsync etc
                //and properties like BaseAddress and DefaultRequestHeaders
                HttpResponseMessage responseMessage = await httpClient.SendAsync(httpRequestMessage);

                //response can be read as a stream
                Stream stream = responseMessage.Content.ReadAsStream();
                StreamReader streamReader = new StreamReader(stream);

                /*
                    the JSON format text content of the response from the API request,
                    Response Attributes:
                    c   Current price
                    d   Change
                    dp  Percent change
                    h   High price of the day
                    l   Low price of the day
                    o   Open price of the day
                    pc  Previous close price
                    response will be in json KVPs, attributes are keys
                 */
                string responseText = streamReader.ReadToEnd();

                //deserialized converted to a Dictionary collection
                //the content is coming from an external API
                //nullable in case any keys dont have values, or values dont have keys
                responseDictionary = 
                    JsonSerializer.Deserialize<Dictionary<string, object>>(responseText);

                //some simple error handling in case of bad response
                if (responseDictionary is null) 
                {
                    throw new InvalidOperationException("no response from finnhub server");
                }
                else if (responseDictionary.ContainsKey("error"))
                {
                    throw new InvalidOperationException(Convert.ToString(responseDictionary["error"]));
                };
            }

            //at this point the HttpClient is disposed of by the above using statement
            //along with the scoped variables and validations
            //if no error has been thrown, the responseDictionary of originally JSON KVPs
            //is considered valid and returned after being converted into a Dictionary in local memory
            return responseDictionary;
        }
    }
}
