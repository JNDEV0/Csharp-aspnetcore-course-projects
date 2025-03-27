﻿﻿﻿﻿﻿﻿﻿﻿﻿//important to note that a lot of these examples are briefly implemented in place for the sake of explaining their working,
//the implementation logic here is not ideal for real life scenarios, for example fetching the full list to filter down
//as often done here is obviously not ideal, instead a real implementation would involve database access services
//built to query the database selectively, but the principles of validation and logic behind the service goal remains the same

//note that the CustomService type classes are NOT writing anything to memory, where a temporary runtime list is suggested there is a DbContext that is used to perform CRUD operations on, see Entities.PersonsDbContext.cs

using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        //for brevity sake in this example a simple list is stored here in CountriesService
        //in real world this collection would be in a database and AddCountry would call some scoped WriteToDatabase type service
        //local runtime temp list commented out after the CustomDbContext below has been implemented
        //private List<Country> _countries;

        //In this example project the CRUD operations were implemented first with a mock runtime database, now with the CustomDbContext dependency injected to the CustomService class, the LINQ queries are converted into the relevant equivalent database prompt by entity framework that internally uses ado.net, to output the query string, in this case SQL query
        //to actually use the DbContext to interact with the database instead of local runtime memory, the CustomService class(this) Services.CountriesService and Services.PersonsService will inject the builder.Services added CustomDbContext at their own constructors and use it for CRUD operations to interact with database via LINQ queries

        //note the CRUD implementations is manually set conditionally for each CustomClassService operation type through the LINQ-to-Query conversion performed internally by EFCore through .net implementation of ADO.NET, that will output the equivalent SQL query instead of local runtime collection operation, The point is interacting with the actual database through the DbContext can be thought of as an adaptation of working with a local runtime collection type.

        //so moving from a mockup runtimeDBcollection to the DbContext that actually accesses a target database may break some operations, in the CountryAddRequest operation the Count method is used instead of Where to retrieve the countries count for example,
        //another example instead of localRuntimeTempList.Get() country from a local runtime memory collection, the PersonDbContext already has a Countries Set collection to store relevant table entries for the Country type of the other table/collection called Persons, so a country instance being fetched through DbContext runtime set corresponding property that corresponds to a database table is beeing retrieved used the customDbContext injected to constructor,
        //then calling SaveChanges passes the generated SQL query to the database provider, that will output results through the LINQ queries
        //for example _dbContext.Countries.FirstOrDefault() would be converted into equivalent SELECT FROM WHERE ORDERBY type query, or even _dbContext.Countries.Where().OrderBy().Select() also would, LINQ to query offers a versatile way of querying the database provider through the EFCore DbContext internal package conversions.
        private readonly PersonsDbContext _dbContext;

        //note that placeholder/seed data is being generated and inserted elsewhere where Services.Utils.MockDataUtil is being called
        public CountriesService(PersonsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        //parameterless constructor used by runtime mockdata example alternative
        public CountriesService()
        {
            //_countries = new List<Country>();
        }

        public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
        {
            //note that the tests executed in CRUDTests.CountriesServiceTests.cs
            //which are visible in the test explorer, according to TDD (TEST DRIVEN DEVELOPMENT)
            //determine the needs of the implementation here,
            //the point is the requirements are outlined in the test and validated by implementation

            //where tests expect a specific error to be thrown for example,
            //it will only pass if the conditions to cause the throw are met
            //it may seem backwards that the implementation depends on the test
            //but it is a good way of planning and ensuring needs are met
            
            //is request DTO null?
            if (countryAddRequest is null)
            {
                throw new ArgumentNullException($"{nameof(countryAddRequest)} DTO object is null");
            }
            //valid name length?
            if (countryAddRequest.Name.Length <= 2)
            {
                throw new ArgumentException("Country name must be at least 2 characters long");
            }
            //is CountryName already in collection?
            if (await _dbContext.Countries.AnyAsync(country => country.Name == countryAddRequest.Name))
            {
                throw new ArgumentException($"{countryAddRequest.Name} is already in the countries collection.");
            }

            //at this point assuming the request data is all validated
            //the guid is generated ToCountry class, the DTO already contains the name
            Country country = countryAddRequest.ToCountry();
            _dbContext.Countries.Add(country); //adding the runtime instance to the DbSet of runtimeDbContext
            await _dbContext.SaveChangesAsync(); //reads the changes of DbSets of the target CustomDbContext class, parses through ADO.NET to update database provider, note that the Add() call does NOT add to the actual database, SaveChanges() needs to be called.

            //returning CountryResponse DTO object with GUID and name
            return country.ToCountryResponse();
        }

        public async Task<List<CountryResponse>> GetCountriesList()
        {
            //using Select() to execute ToCountryResponse() for each country
            //select return IEnumerable, so calling ToList() and returning
            return await _dbContext.Countries.Select(country => country.ToCountryResponse()).ToListAsync();

        }

        public async Task<CountryResponse?> GetCountryById(Guid? countryId)
        {
            //if unput id is null, there is no country to get
            if (countryId is null) return null;

            //if id exists but is not in collection
            //default is null, by default
            Country? country = await _dbContext.Countries.FirstOrDefaultAsync((country) => country.CountryId.ToString() == countryId.ToString());
            if (country is null) return null;
            
            //if country exists, return valid CountryResponse
            return country.ToCountryResponse();
        }
    }
}
