/* 
 * 
 * //important to note that a lot of these examples are briefly implemented in place for the sake of explaining their working,
//the implementation logic here is not ideal for real life scenarios, for example fetching the full list to filter down
//as often done here is obviously not ideal, instead a real implementation would involve database access services
//built to query the database selectively, but the principles of validation and logic behind the service goal remains the same

//note the disparity between the method implementations, some use PersonResponse and some use Person property verifications
//this can be a problem or intentional, since the Domain Person should never be accessed directly from the client
//having different property names can be intentional for further obfuscation of server side logic,
//in this example the property names are the same, so the verifications used work interchangeably, since they work on property name

In this example project the CRUD operations were implemented first with a mock runtime database, now with the CustomDbContext dependency injected to the CustomService class, the LINQ queries are converted into the relevant equivalent database prompt by entity framework that internally uses ado.net, to output the query string, in this case SQL query

note the CRUD implementation is manually implemented conditionally for each CustomClassService type, so moving from a mockup runtimeDB to the DbContext that actually accesses a target database may break some operations, here in the CountryAddRequest operation the Count method is used instead of Where to retrieve the countries 

additional changes to accomodate actual database code-first generation, instead of simply .Add() the country to the local runtimeDB, here the DbContext already has a Countries Set, so target country being added is added to the DbContext set, and SaveChanges is called to actually pass the generated SQL Add statement to the database and save the generated operation

internally entity framework core sets states to entities as they are modified in the DbSet targets, so a property value changing on an entity of a DbSet will be flagged internally to update so when SaveChanges is called, the update operation actually occurs.
*/

//this class initially utilized runtime memory mockup data, so the classes did not need to be asynchronous, since the application did not communicate externally, now the methods are being converted to async/await so this application can make async requests and await their responses to the external database provider

using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Utils;

namespace Services
{
    public class PersonsService : IPersonsService
    {
        //for brevity sake in this example a simple list is stored here in PersonsService
        //in real world this collection would be in a database and AddPerson would call
        //some scoped WriteToDatabase type service for example
        //private List<Person> _persons;

        //In this example project the CRUD operations were implemented first with a mock runtime database, now with the CustomDbContext dependency injected to the CustomService class, the LINQ queries are converted into the relevant equivalent database prompt by entity framework that internally uses ado.net, to output the query string, in this case SQL query
        //to actually use the DbContext to interact with the database instead of local runtime memory, the CustomService class(this) Services.CountriesService and Services.PersonsService will inject the builder.Services added CustomDbContext at their own constructors and use it for CRUD operations to interact with database via LINQ queries

        //note the CRUD implementations is manually set conditionally for each CustomClassService operation type through the LINQ-to-Query conversion performed internally by EFCore through .net implementation of ADO.NET, that will output the equivalent SQL query instead of local runtime collection operation, The point is interacting with the actual database through the DbContext can be thought of as an adaptation of working with a local runtime collection type.

        //so moving from a mockup runtimeDBcollection to the DbContext that actually accesses a target database may break some operations, in the CountryAddRequest operation the Count method is used instead of Where to retrieve the countries count for example,
        //another example instead of localRuntimeTempList.Get() country from a local runtime memory collection, the PersonDbContext already has a Countries Set collection to store relevant table entries for the Country type of the other table/collection called Persons, so a country instance being fetched through DbContext runtime set corresponding property that corresponds to a database table is beeing retrieved used the customDbContext injected to constructor,
        //then calling SaveChanges passes the generated SQL query to the database provider, that will output results through the LINQ queries
        //for example _dbContext.Countries.FirstOrDefault() would be converted into equivalent SELECT FROM WHERE ORDERBY type query, or even _dbContext.Countries.Where().OrderBy().Select() also would, LINQ to query offers a versatile way of querying the database provider through the EFCore DbContext internal package conversions.
        private readonly PersonsDbContext _dbContext;

        //used by mockDataUtil
        private ICountriesService _countriesService;

        //the constructor parameter is to determine weather or not to use made up data for testing
        //note that the PersonServiceTests gives this as false
        //mockdata is a simplified way of injecting a few countries and persons into the runtime memory for testing

        //public PersonsService(PersonsDbContext dbContext, ICountriesService countriesService)
        public PersonsService(PersonsDbContext dbContext)
        {
            _dbContext = dbContext;
            //_countriesService = countriesService;
        }

        //no dbContext constructor used by runtime mockdata example alternative
        //public PersonsService(ICountriesService countriesService)
        //{
        //    _countriesService = countriesService;
        //}

        //external services like database, email server, auth server etc, any contact to a program/server outside a given solution/application should use async/await methods to allow the calling application to properly pause execution of a given request while await a response from external source, and continue handling other requests, in separate async/await request threads.
        //the point of using async await is to allow a thread handling a request to pause and wait for an Input/Output response of an external resource/server
        //that is NOT synchronized with this server's current linear execution. this allows server hardware resources to be reallocated to handle other requests while the server awaits the response on a given request's operations.
        //this is the reason why await is NOT called in every operation, instead the method itself returns an async Task<> to allow the internal multi-threading to handle the request, and only await at IO external resource, in this case the database provider through the _dbContext
        public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest)
        {
            //note that the tests executed in CRUDTests.PersonsServiceTests.cs
            //which are visible in the test explorer, according to TDD (TEST DRIVEN DEVELOPMENT)
            //determine the needs of the implementation here,
            //the point is the requirements are outlined in the test and validated by implementation

            //where tests expect a specific error to be thrown for example,
            //it will only pass if the conditions to cause the throw are met
            //it may seem backwards that the implementation depends on the test
            //but it is a good way of planning and ensuring needs are met

            //is request DTO null?
            if (personAddRequest is null)
            {
                throw new ArgumentNullException($"{nameof(personAddRequest)} DTO object is null");
            }

            //validation can be done via model validation binding rules
            //for this method, data annotations are added to the PersonAddRequest to demonstrate
            //see model related projects for details on model binding
            ValidationUtil.ModelValidation(personAddRequest);

            ////valid name length?
            //if (personAddRequest.Name.Length <= 2)
            //{
            //    throw new ArgumentException("Person name must be at least 2 characters long");
            //}

            //is CountryName already in collection?
            //note that after the ICustomService interface is updated to return async Tasks<> from thisCustomService class, it needs to be updated to properly use the async/await functions when interacting with IO external services,
            if (await _dbContext.Persons.CountAsync(person => person.Name == personAddRequest.Name) > 0)
            {
                throw new ArgumentException($"{personAddRequest.Name} is already in the persons collection.");
            }

            //at this point assuming the request data is all validated
            //the person add request has an implemented method that when called parses out the data from the request
            //into a Person object, who in turn has a constructor that instantiates a new Guid
            //note that most property fields are nullable
            Person person = personAddRequest.ToPerson();

            Country? relatedCountry = await _dbContext.Countries.FirstOrDefaultAsync(country => country.CountryId == person.CountryId);
            person.CountryName = relatedCountry.Name;

            //this implementation of using the efcore automation can be preferable, however it is commented out below to showcase the StoredProcedure that has been added to the databse provider via migration, see Entities.Migrations folder.
            //note that the method whos result are returned by StoredProcedure_AddPerson, returns an int of number of rows affected, can be used to further validate the operation perhaps
            //IMPORTANT that calling the stored procedure in database bypasses the change detection/saving mechanism of EfCore, meaning that changes made by the AddPerson call need not be "saved" by calling _dbContext.SaveChanges or .SaveChangesAsync here,
            //the stored procedure is a send-and-forget type interaction, where the given object is passed to database provider and no response 
            _dbContext.StoredProcedure_AddPerson(person);

            //_dbContext.Persons.Add(person); //by adding to the DbSet<Person> collection called Persons, the change will be detected by efcore HOWEVER it is not saved a the actual database provider, here is it only added to the runtime DbSet collection
            //_dbContext.SaveChanges(); //non-read CRUD operations need to be saved for changes to be saved to database provider

            //returning CountryResponse DTO object with GUID and name
            return person.ToPersonResponse();
        }

        //continuing on the same comments a few lines above about the storedProcedure, since this GetPersonsList() method does NOT have a stored procedure
        //instead uses the EfCore automation changes needs to be saved using SaveChangesAsync()
        public async Task<List<PersonResponse>> GetPersonsList()
        {
            //using Select() to execute ToCountryResponse() for each country
            //select return IEnumerable, so calling ToList() and returning
            //return _dbContext.Persons.Select(person => person.ToPersonResponse()).ToList();

            //the stored procedure in the database is a way of retrieving the data as well, but here it is commented out because the stored procedure does not automatically update its raw SQL to changes made in the data model, so changing properties in Person in this case will need to be migrated to database and then a new migration to update GetAllPersons stored procedure
            //return _dbContext.StoredProcedure_GetAllPersons().Select(person => person.ToPersonResponse()).ToList();

            //note that the CustomService class that uses the CustomDbContext calls Include("PropertyName") to populate the values of the nested Country object of Person, based on the[ForeignKey] annotation that points to the relevant CountryId property in Person.cs
            //a "navigation property" as it stipulates the object relation in the parent object for easy access on demand, rather than having to join multiple separate tables manually in an Sql Query, EFcore automates the process through the config either in PersonsDbContext or [tags] in model class
            List<PersonResponse> persons = await _dbContext.Persons.Include("Country").Select(person => person.ToPersonResponse()).ToListAsync();
            
            //note the persons variable above retrieves the populated Persons list, with the populated Country nav property, so the Country details are already included in the Person instance, and then converted one by one using Select ToPersonResponse, that will attempt to set the values of CountryId and CountryName using the stored nav propert of Country
            return persons;
        }

        public async Task<PersonResponse?> GetPersonById(Guid? personId)
        {
            //if unput id is null, there is no person to get
            if (personId is null) return null;

            //if id exists but is not in collection, default is null for FirstOrDefault
            //note also the Include("Country") call, that will populate the Country nav property of Persons class
            Person? person = await _dbContext.Persons.Include("Country").FirstOrDefaultAsync((persons) => persons.PersonId == personId);
            if (person is null) return null;
            
            //if country exists, return valid CountryResponse
            return person.ToPersonResponse();
        }

        public async Task<List<PersonResponse>> GetPersonsBy(string propertyName, string? propertyValue)
        {

            //if no search property, return all
            if (propertyName is null || propertyValue is null) return await GetPersonsList();

            List<Person> matchingPersons = new();
            //a side note on this switch, properties are all null checked because of previous implementation choices, of having a common Data Transfer Object for various operations, meaning most person objects in the actual collection may have null values in any given field
            //also note that await is not used in these switches operations, since the list is already retrieved to local matchinPersons variable
            switch (propertyName)
            {
                case nameof(PersonResponse.PersonId):
                    //match (temp)elements of all GetPersonsList() where the person.PersonId
                    //is not null and matches the guid to string to propertyValue query 
                    //other cases below follow similar logic, fetching from all persons list
                    //elements that match the validation, keeping in mind that propertValue is in string format
                    //and the condition check is based on the property type
                    matchingPersons = await _dbContext.Persons
                        .Where(temp => temp.PersonId != null && //note that using {} codeblock body here breaks the linq-efcore integration
                           temp.PersonId.ToString().Contains(propertyValue, StringComparison.OrdinalIgnoreCase))
                        .ToListAsync(); break;
                case nameof(PersonResponse.Dob):
                    matchingPersons = await _dbContext.Persons
                        .Where(temp => temp.Dob != null &&
                           temp.Dob.Value.ToString("dd MMMM yyyy").Contains(propertyValue, StringComparison.OrdinalIgnoreCase))
                        .ToListAsync(); break;
                case nameof(PersonResponse.Name):
                    //match (temp)elements of all GetPersonsList() where the person.etc 
                    //same logic
                    matchingPersons = await _dbContext.Persons
                        .Where(temp => temp.Name != null &&
                        temp.Name.Contains(propertyValue, StringComparison.OrdinalIgnoreCase))
                        .ToListAsync(); break;
                case nameof(PersonResponse.Gender):
                    matchingPersons = await _dbContext.Persons
                        .Where(temp => !string.IsNullOrEmpty(temp.Gender.ToString()) &&
                                       temp.Gender.ToString().Equals(propertyValue, StringComparison.OrdinalIgnoreCase))
                        .ToListAsync(); break;
                case nameof(PersonResponse.CountryId):
                    matchingPersons = await _dbContext.Persons
                        .Where(temp => !string.IsNullOrEmpty(temp.CountryId.ToString()) &&
                                       temp.CountryId.ToString().Contains(propertyValue, StringComparison.OrdinalIgnoreCase))
                        .ToListAsync(); break;
                case nameof(PersonResponse.CountryName):
                    matchingPersons = await _dbContext.Persons
                        .Where(temp => !string.IsNullOrEmpty(temp.CountryName) &&
                                       temp.CountryName.Contains(propertyValue, StringComparison.OrdinalIgnoreCase))
                        .ToListAsync();
                    break;
                case nameof(PersonResponse.Address):
                    matchingPersons = await _dbContext.Persons
                        .Where(temp => !string.IsNullOrEmpty(temp.Address) &&
                                       temp.Address.Contains(propertyValue, StringComparison.OrdinalIgnoreCase))
                        .ToListAsync();
                    break;
                case nameof(PersonResponse.Email):
                    matchingPersons = await _dbContext.Persons
                        .Where(temp => !string.IsNullOrEmpty(temp.Email) &&
                                       temp.Email.Contains(propertyValue, StringComparison.OrdinalIgnoreCase))
                        .ToListAsync();
                    break;
                case nameof(PersonResponse.ReceiveNewsLetters):
                    matchingPersons = await _dbContext.Persons
                        .Where(temp => temp.ReceiveNewsLetters)
                        .ToListAsync();
                    break;
                default: break;
            }

            //after the filtering through the switch above, the matchingPersons will either be
            //all persons, in case query property is null, or the specific matching property with PARTIAL match
            //GetByExact() and GetByPartial(), for brevity just returning the output
            //conversion from Person to PersonResponse is on the local memory matchinPersons and so is not using async method variants
            return matchingPersons.Select(person => person.ToPersonResponse()).ToList();
        }

        public async Task<List<PersonResponse>> SortPersons(List<PersonResponse> unsortedList, string sortByPropertyName, SortOrderEnum sortDirection)
        {
            if (string.IsNullOrEmpty(sortByPropertyName)) return unsortedList;

            //using switch expression with tuple of propertyName and direction
            List<PersonResponse> sortedList = 
                (sortByPropertyName, sortDirection) switch
            {
            //the parameters of the lambda expression are the switch case
            //done this way just to illustrate the use of switch case with lambda expression for simple return values
            (nameof(PersonResponse.PersonId), SortOrderEnum.Descending) =>
                unsortedList.OrderByDescending(person => person.PersonId.ToString(), StringComparer.OrdinalIgnoreCase).ToList(),
            (nameof(PersonResponse.PersonId), SortOrderEnum.Ascending) =>
                unsortedList.OrderBy(person => person.PersonId.ToString(), StringComparer.OrdinalIgnoreCase).ToList(),
            
            (nameof(PersonResponse.Name), SortOrderEnum.Descending) => 
                unsortedList.OrderByDescending(person => person.Name, StringComparer.OrdinalIgnoreCase).ToList(),
            (nameof(PersonResponse.Name), SortOrderEnum.Ascending) =>
                unsortedList.OrderBy(person => person.Name, StringComparer.OrdinalIgnoreCase).ToList(),

            (nameof(PersonResponse.Email), SortOrderEnum.Descending) =>
                unsortedList.OrderByDescending(person => person.Email, StringComparer.OrdinalIgnoreCase).ToList(),
            (nameof(PersonResponse.Email), SortOrderEnum.Ascending) =>
                unsortedList.OrderBy(person => person.Email, StringComparer.OrdinalIgnoreCase).ToList(),

            (nameof(PersonResponse.Dob), SortOrderEnum.Descending) =>
                unsortedList.OrderByDescending(person => person.Dob).ToList(),
            (nameof(PersonResponse.Dob), SortOrderEnum.Ascending) =>
                unsortedList.OrderBy(person => person.Dob).ToList(),
            
            (nameof(PersonResponse.Gender), SortOrderEnum.Descending) =>
                unsortedList.OrderByDescending(person => person.Gender.Value).ToList(),
            (nameof(PersonResponse.Gender), SortOrderEnum.Ascending) =>
                unsortedList.OrderBy(person => person.Gender.Value).ToList(),
            
            (nameof(PersonResponse.CountryName), SortOrderEnum.Descending) =>
                unsortedList.OrderByDescending(person => person.CountryId).ToList(),
            (nameof(PersonResponse.CountryName), SortOrderEnum.Ascending) =>
                unsortedList.OrderBy(person => person.CountryId).ToList(),
            
            (nameof(PersonResponse.Address), SortOrderEnum.Descending) =>
                unsortedList.OrderByDescending(person => person.Address, StringComparer.OrdinalIgnoreCase).ToList(),
            (nameof(PersonResponse.Address), SortOrderEnum.Ascending) =>
                unsortedList.OrderBy(person => person.Address, StringComparer.OrdinalIgnoreCase).ToList(),
            
            (nameof(PersonResponse.ReceiveNewsLetters), SortOrderEnum.Descending) => 
                unsortedList.OrderByDescending(person => person.ReceiveNewsLetters).ToList(),
            (nameof(PersonResponse.ReceiveNewsLetters), SortOrderEnum.Ascending) =>
                unsortedList.OrderBy(person => person.ReceiveNewsLetters).ToList(),
            
                _ => unsortedList //default is the unsorted list
            };
            
            return sortedList;
        }

        public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest)
        {
            if (personUpdateRequest is null) throw new ArgumentNullException("Invalid null request");

            //instead of validating each field here, the ModelValidation utility is used
            //will throw argument Exception if any model validations fail
            Utils.ValidationUtil.ModelValidation(personUpdateRequest);

            Person? targetPerson = await _dbContext.Persons.Include("Country").FirstOrDefaultAsync((person) => person.PersonId == personUpdateRequest.PersonId);
            if (targetPerson is null) throw new ArgumentException("Invalid personId");

            //while tempting, the code below will NOT work!
            //since EFcore tracks entity property changes, the targetPerson's properties need to be individually changed,
            //Person? newPerson = personUpdateRequest.ToPerson();
            //targetPerson = newPerson;
            targetPerson.Name = personUpdateRequest.Name;
            targetPerson.Email = personUpdateRequest.Email;
            targetPerson.Dob = personUpdateRequest.Dob;
            targetPerson.Gender = personUpdateRequest.Gender.ToString();
            targetPerson.CountryId = personUpdateRequest.CountryId;
            targetPerson.Address = personUpdateRequest.Address;
            targetPerson.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;


            //given that changes need to be saved to database context/provider using SaveChangesAsync() to ensure that any async responses are properly accounted for when changes in runtime data are concluded and can be saved to database
            //this method has a non-async equivalent SaveChanges()
            await _dbContext.SaveChangesAsync(); //non-read CRUD operations need to be saved for changes to be saved to database provider
            return personUpdateRequest.ToPerson().ToPersonResponse();
        }

        public async Task<bool> DeletePerson(PersonDeleteRequest? personDeleteRequest)
        {
            //note that there is no need to null check here, since modelValidation will resolve it
            //via property annotations
            //if (personDeleteRequest is null) throw new ArgumentNullException();

            //will throw an argument null exception if the PersonId is not present
            ValidationUtil.ModelValidation(personDeleteRequest);
            
            Person? target = await _dbContext.Persons.FirstOrDefaultAsync((person) => person.PersonId == personDeleteRequest.PersonId);
            if (target is null) throw new ArgumentException("person was not found on list");
            else
            {
                //note that NOT ALL calls to dbContext are async, since Remove() in fact just logs the change in EFcore, it is not an IO external access event
                _dbContext.Persons.Remove(target);

                //while saving the changes to the external database provider is async, through the same _dbContext
                await _dbContext.SaveChangesAsync(); //non-read CRUD operations need to be saved for changes to be saved to database provider
                return true;
            }
        }
    }
}
