//leaving this unused import to point out that Country.cs is a domain level model,
//so in fact Entities.Country is never used directly, interactions are through the corresponding DTOs
using Entities; 

//import relevant dependencies
using ServiceContracts; //Dependency Injection interfaces for Services
using ServiceContracts.DTO;
using Services; //Data transfer Objects, wrappers for the request

namespace CRUDTests
{
    public class CountriesServiceTests
    {
        private readonly ICountriesService _countriesService;

        public CountriesServiceTests() 
        {
            //simply instantiating the test class here,
            //in production the service class could be added to dependency injection of builder.Services
            //and injected via constructor parameter
            _countriesService = new CountriesService();
        }

        #region AddCountry
        //this first test showcases why the Data Transfer Object is useful
        //the request DTO contains the incoming request countryName, no Guid
        //the response will contain, upon processing a valid request,
        //the generated Guid by AddCountry() and the original CountryName
        [Fact]
        public async Task AddCountry_ValidRequest_Test()
        {
            CountryAddRequest? request = new CountryAddRequest() { Name = "ValidTest" };
            CountryResponse response = await _countriesService.AddCountry(request);
            List<CountryResponse> getAllCountries = await _countriesService.GetCountriesList();
            Assert.True(response.CountryId != Guid.Empty && response.Name == "ValidTest");

            //an interesting note on the Contains method, it calls the Equals() method of the class,
            //by default it does not compare the actual object data/properties.
            //by default the statement below fails even though both references
            //point to same object with same data
            //so we override Equals() method of target class to enable Contains() to work properly
            //see CountryResponse.cs
            Assert.Contains(response, getAllCountries);
        }

        [Fact]
        public async Task AddCountry_InvalidName_Test()
        {
            CountryAddRequest badRequest = new CountryAddRequest() { Name = "" };

            //test if target method will throw the exception given empty CountryName
            await Assert.ThrowsAsync<ArgumentException>(async () => 
            {
                await _countriesService.AddCountry(badRequest);
            });
        }

        //all unit tests have the [Fact] attribute which signals to xUnit that this is a test case
        //an Assert.x statement that will define the result, not that return type of test method is void
        //all components needed for the test are contained/instantiated within it using made-up variables
        //for details that are not relevant to the specific implementation being tested
        [Fact]
        public async Task AddCountry_NullRequest_Test()
        {
            //aranging Data Transfer Object 
            CountryAddRequest? nullRequest = null;

            //this method asserts wether the code executed inside the lambda expression
            //throws an exception as expected, in this cause, because the DTO is actually null
            //note that a passing test does not always mean the target method works completely!
            //this test only confirms the ArgumentNullException check of the target unit tested method
            await Assert.ThrowsAsync<ArgumentNullException>(async () => 
            {
                await _countriesService.AddCountry(nullRequest);
            });
        }

        [Fact]
        public async Task AddCountry_DuplicateCountryName_Test() 
        { 
            CountryAddRequest? request1 = new CountryAddRequest() { Name = "Test" };
            CountryAddRequest? request2 = new CountryAddRequest() { Name = "Test" };

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _countriesService.AddCountry(request1);
                await _countriesService.AddCountry(request2);
            });
        }
        #endregion

        #region GetCountriesList
        [Fact]
        public async Task GetCountriesList_CorrectReturnType_Test()
        {
            var addRequests = new List<CountryAddRequest>()
            {
                new CountryAddRequest("Uzbekistan"),
                new CountryAddRequest("Turkmanistan"),
                new CountryAddRequest("IriquoisNation"),
                new CountryAddRequest("Mongolia")
            };

            //the expected list will contain the list of references to the same
            var expected = new List<CountryResponse>();
            for (int i = 0; i < addRequests.Count; i++)
            {
                expected.Add(await _countriesService.AddCountry(addRequests[i]));
            }
            
            //objects that should be returned from GetCountriesList()
            var actual = await _countriesService.GetCountriesList();
            
            //if any check fails, the entire test fails,
            //passes only if all checks pass
            //note that Contains() calls Equals() method of target class being compared
            //must be overriden to verify actual object fields instead of object literals by default
            foreach (CountryResponse country in expected)
            {
                Assert.Contains(country, actual);
            }
        }

        [Fact]
        public async Task GetCountriesList_EmptyList_Test()
        {
            //is default GetList working with empty list?
            List<CountryResponse> actual = await _countriesService.GetCountriesList();

            Assert.Empty(actual);
        }
        #endregion

        #region GetCountry
        [Fact]
        public async Task GetCountryById_ValidRequest_Test()
        {
            CountryAddRequest addRequest = new CountryAddRequest("Mongolia");

            CountryResponse expected = await _countriesService.AddCountry(addRequest);

            CountryResponse actual = await _countriesService.GetCountryById(expected.CountryId);


            //the statement below would also work
            //but since Equals() method is already overriden
            //Assert.Equal() will validate the object properties correctly
            //Assert.True(expected.Name == actual.Name);
            Assert.Equal(expected, actual);
        }

        //testing if given a null object instead of valid Guid
        //countryId if returns null
        [Fact]
        public async Task GetCountryById_NullRequest_Test()
        {
            CountryResponse? expected = await _countriesService.GetCountryById(null);

            Assert.Null(expected);
        }
        #endregion
    }
}
