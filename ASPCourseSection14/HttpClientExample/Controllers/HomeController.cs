using HttpClientExample.Models.Options;
using HttpClientExample.Models.ViewModels;
using HttpClientExample.ServiceContracts;
using HttpClientExample.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HttpClientExample.Controllers
{
    public class HomeController : Controller
    {
        private IHttpClientService _httpClientService { get; }

        //accessing the environment variable from appsettings.json using IOptions
        //by stipulating the target model class StockQuote, the IOptions that is injected
        //takes on class name as the target master key, and the fields take on value of nested keys
        //note the information stored this way is not secure as it is readable/editable directly at appsettings.json ->
        public IOptions<StockQuote> _options { get; }

        public HomeController(IHttpClientService httpClientService, IOptions<StockQuote> options)
        {
            _httpClientService = httpClientService;
            _options = options;
        }

        //note that instead of injecting to the constructor and storing in a field,
        //could be injected directly into the actionMethod parameter (HttpClientService httpClientService)
        [Route("/")]
        public async Task<IActionResult> Index() //HttpClientService httpClientService
        {
            //in this example we are accessing finnhub.io API for stock price details
            //the return type of the GetStockPriceQuote is actually Task<Dictionary<string,object>?>
            //since the await keyword is used, the Task will collect the response and asyncronously returns the Dictionary object
            //parsed from the response, this API happens to return in this dictionary format

            //debug: the the options will instantiate the target StockQuote class and give its field a null value
            //IF the class is not added as a IOptions<> service, this is so that the server will bind to target StockQuote
            //the data retrieved from appsettings.json, note the names and property fields must match the KVPs
            //Console.WriteLine("defaultstockSymbol from model: " + _options.Value.DefaultStockSymbol);

            //note that the stocksymbol is being retrieved from appsettings.json as a default value
            //inside GetStockPriceQuote() the secure API access token is being retrieved from local dev machine user-secrets
            Dictionary<string, object> response = await _httpClientService.GetStockPriceQuote(_options.Value.DefaultStockSymbol);

            //note that Models/Options/StockQuote is another class that is only used to
            //bind the target JSON KVP to a data model instance, 
            //StockQuoteData is the model for the successfull response from the API
            //this is not the correct way to bind the service result, done here for example brevity
            StockQuoteData stockQuoteData = new StockQuoteData()
            {
                //note that the direct response["key"] retrieved value is still of Text.Json.JsonElement type
                //and so is first converted into a string, then to a double
                StockSymbol = _options.Value.DefaultStockSymbol,
                CurrentPrice = Convert.ToDouble(response["c"].ToString()),
                HighestPrice = Convert.ToDouble(response["h"].ToString()),
                LowestPrice = Convert.ToDouble(response["l"].ToString()),
                OpenPrice = Convert.ToDouble(response["o"].ToString())
            };

            //now the data has been retrieved from user-secrets environment variables, local app configuration KVPs from appsettings.json,
            //compiled the request sent async to the external HTTP API service finnhub.io, parsed the JSON response into local memory data
            //finally can be passed to the view via ViewBag or as data model parameter
            //ViewBag.StockQuoteData = stockQuoteData;

            return View(stockQuoteData);
        }
    }
}
