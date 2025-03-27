using ServiceContracts.DTO;

namespace ServiceContracts
{
    //will be useful for dependency injection of the services into the app builder
    //represents business logic for manipulating country service instance through Inversion of Control container
    public interface ICountriesService
    {
        /// <summary>
        /// Add country to the list of countries
        /// </summary>
        /// <param name="countryAddRequest">request of country to be added</param>
        /// <returns>contains Guid and Name, confirmation of operation</returns>
        public Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest);

        /// <summary>
        /// Get list of all countries in CountryResponse DTOs
        /// </summary>
        /// <returns></returns>
        public Task<List<CountryResponse>> GetCountriesList();

        /// <summary>
        /// attempt to get a country's details through CountryResponse DTO
        /// </summary>
        /// <param name="countryId"></param>
        /// <returns></returns>
        public Task<CountryResponse?> GetCountryById(Guid? countryId);
    }
}
