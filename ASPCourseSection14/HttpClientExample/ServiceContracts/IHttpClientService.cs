namespace HttpClientExample.ServiceContracts
{
    public interface IHttpClientService
    {
        Task<Dictionary<string, object>?> GetStockPriceQuote(string stockSymbol);
    }
}
