//note that an actual implementation should have proper constructors for the various uses of the DTO class, with the attribute validation for each field, here for example brevity the fields are initialized directly in the expecte format, but user input is not reliable

//an insteresting point of this example project, is that the development is test driven, meaning
//the implementation was done in the order of Arrange, Act, Assert,
//first the method inputs and outputs are defined in the service interface, in this case IPersonsService 
//that will cause the PersonsService that implements it to fail the implementation, so the new method is added
//by default it throws NotImplementedException, then a minimal implementation of the method's logic is provided
//to elaborate test cases in PersonsServiceTests.cs,
//for example a simple null check logic is all that is needed for a ThrowsIfNull_Test, so the point is
//implementation is driven by the test cases needs, met by the piecemeal implemented components of the test

using Entities; //Person class
using Microsoft.EntityFrameworkCore;
using ServiceContracts; //Dependency Injection interfaces for Services
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using Xunit.Abstractions; //Data transfer Objects, wrappers for the request

namespace CRUDTests
{
    public class PersonsServiceTests
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService; //used to simulate existing countries for person related requests

        //test suite classes can use an xUnit injected ITestOutputHelper parameter in the constructor,
        //which functions similar to Console.Writeline would in execution, but for unit tests
        //this can help identify what is failing in tests that may otherwise not be informative
        //prints in the test summary that appears in test explorer
        private readonly ITestOutputHelper _outputHelper;

        public PersonsServiceTests(ITestOutputHelper testOutputHelper) 
        {
            //simply instantiating the test class here,
            //in production the service class could be added to dependency injection of builder.Services
            //and injected via constructor parameter
            _countriesService = new CountriesService();

            //previously inputed CountriesService class directly to PersonsService, now using a CustomDbContext that required Options to point to db provider
            _personsService = new PersonsService(new PersonsDbContext(new DbContextOptionsBuilder<PersonsDbContext>().Options));
            _outputHelper = testOutputHelper;
        }

        #region AddPerson
        //this first test showcases why the Data Transfer Object is useful
        //the request DTO contains the incoming request Persons Name, no Guid
        //the response will contain, upon processing a valid request,
        //the generated Guid by AddPersons() and the original Persons Name
        [Fact]
        public async Task AddPersons_ValidRequest_Test()
        {
            CountryAddRequest countryRequest = new CountryAddRequest("Mongolia");
            CountryResponse countryResponse = await _countriesService.AddCountry(countryRequest);

            PersonAddRequest? request = new PersonAddRequest()
            {
                Name = "ValidName",
                Email = "valid@email.com",
                Address = "123 sample rd",
                CountryId = countryResponse.CountryId,
                Gender = ServiceContracts.Enums.GenderOptions.Male,
                Dob = DateTime.Parse("2010-10-10"),
                ReceiveNewsLetters = true
            };
            PersonResponse response = await _personsService.AddPerson(request);

            Assert.True(
                response.PersonId != Guid.Empty && //we dont have a Guid before the AddPerson assigns one
                response.Name == request.Name &&
                response.Email == request.Email &&
                response.Address == request.Address &&
                response.CountryId == request.CountryId &&
                response.Gender == request.Gender &&
                response.Dob == request.Dob &&
                response.ReceiveNewsLetters == request.ReceiveNewsLetters, $"country: {countryResponse.CountryId} res:{response.CountryId} req:{request.CountryId}"
                );

            //one more verification, the persons list should return the person above
            List<PersonResponse> getAllPersons = await _personsService.GetPersonsList();

            //an interesting note on the Contains method, it calls the Equals() method of the class,
            //by default it does not compare the actual object data/properties.
            //by default the statement below fails even though both references
            //point to same object with same data
            //so we override Equals() method of target class to enable Contains() to work properly
            //see CountryResponse.cs
            Assert.Contains(response, getAllPersons);
        }

        [Fact]
        public async Task AddPersons_InvalidName_Test()
        {
            PersonAddRequest badRequest = new PersonAddRequest() { Name = "" };

            //test if target method will throw the exception given empty CountryName
            await Assert.ThrowsAsync<ArgumentException>(async () => 
            {
                await _personsService.AddPerson(badRequest);
            });
        }

        //all unit tests have the [Fact] attribute which signals to xUnit that this is a test case
        //an Assert.x statement that will define the result, not that return type of test method is void
        //all components needed for the test are contained/instantiated within it using made-up variables
        //for details that are not relevant to the specific implementation being tested
        [Fact]
        public async Task AddPerson_NullRequest_Test()
        {
            //aranging Data Transfer Object 
            PersonAddRequest? nullRequest = null;

            //this method asserts wether the code executed inside the lambda expression
            //throws an exception as expected, in this cause, because the DTO is actually null
            //note that a passing test does not always mean the target method works completely!
            //this test only confirms the ArgumentNullException check of the target unit tested method
            await Assert.ThrowsAsync<ArgumentNullException>(async () => 
            {
                await _personsService.AddPerson(nullRequest);
            });
        }

        [Fact]
        public async Task AddPerson_DuplicateCountryName_Test() 
        { 
            PersonAddRequest? request1 = new PersonAddRequest() { Name = "Test" };
            PersonAddRequest? request2 = new PersonAddRequest() { Name = "Test" };

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _personsService.AddPerson(request1);
                await _personsService.AddPerson(request2);
            });
        }
        #endregion

        #region GetPersonsList
        [Fact]
        public async Task GetPersonsList_CorrectReturnType_Test()
        {
            //Persons have a CountryId type field to store the Guid of their countries
            //note that this entire test fails if name or email is provided, even if other fields are present
            //since the PersonAddRequest has [Required] fields that will throw errors if not provided
            //but this is not immediately apparent since xUnit does not explain what the issue is
            //so outputHelper is useful to see what is happening since Console.Writeline does not work in tests
            //use outputHelper to print test info to Test Detail Summary
            CountryAddRequest countryAddRequest1 = new CountryAddRequest() { Name = "Argelia" };
            CountryAddRequest countryAddRequest2 = new CountryAddRequest() { Name = "Ukraine" };
            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryAddRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(countryAddRequest2);

            var addRequests = new List<PersonAddRequest>()
            {
                new PersonAddRequest() {Name = "Billy", Email = "email11@format.com", CountryId = countryResponse1.CountryId},
                new PersonAddRequest() {Name = "Zippy", Email = "email22@format.com", CountryId = countryResponse2.CountryId},
                new PersonAddRequest() {Name = "Sitty", Email = "email33@format.com", CountryId = countryResponse1.CountryId},
                new PersonAddRequest() {Name = "Beanie", Email = "email44@format.com", CountryId = countryResponse2.CountryId}
            };

            //the expected list will contain the list of references to the same
            var expected = new List<PersonResponse>();
            for (int i = 0; i < addRequests.Count; i++)
            {
                expected.Add(await _personsService.AddPerson(addRequests[i]));
            }
            
            //objects that should be returned from GetCountriesList()
            List<PersonResponse> actual = await _personsService.GetPersonsList();
            
            //here the personIds of the expected successfully added persons ie expectedPerson
            //has the PersonId property compared the actual returned from the stored list
            //this output using outputHelper will show in the test explorer window

            //note that _outputHelper is not tied to any internal logic on weather pass/fail
            //the output will show a "Standard Output" section in test detail summary in test explorer
            //with the output, weather test passes or fails, UNLESS logic is added to output conditionally
            //foreach (PersonResponse expectedPerson in expected)
            //{
            //    //could override the PersonResponse ToString method to output its own content instead
            //    _outputHelper.WriteLine($"exp:{expectedPerson.PersonId} act:{actual.Find((person) => person.PersonId == expectedPerson.PersonId).PersonId}");
            //}

            //if any check fails, the entire test fails,
            //passes only if all checks pass
            //note that Contains() calls Equals() method of target class being compared
            //must be overriden to verify actual object fields instead of object literals by default
            foreach (PersonResponse person in expected)
            {
                Assert.Contains(person, actual);
            }
        }

        //[Fact]
        //public void GetPersonsList_EmptyList_Test()
        //{
        //    //is default GetList working with empty list?
        //    //for this test to work, disable useMockData in PersonsService
        //    List<PersonResponse> actual = _personsService.GetPersonsList();

        //    Assert.Empty(actual);
        //}
        #endregion

        #region GetPerson
        [Fact]
        public async Task GetPersonById_ValidRequest_Test()
        {
            PersonAddRequest addRequest = new PersonAddRequest() { Name = "Zimbabwe", Email = "email55@format.com", Address = "123 street", ReceiveNewsLetters = true};

            PersonResponse expected = await _personsService.AddPerson(addRequest);

            PersonResponse actual = await _personsService.GetPersonById(expected.PersonId);

            //the statement below would also work
            //but since Equals() method is already overriden
            //Assert.Equal() will validate the object properties correctly
            //Assert.True(expected.Name == actual.Name);
            Assert.Equal(expected, actual);
        }

        //testing if given a null object instead of valid Guid
        //countryId if returns null
        [Fact]
        public async Task GetPersonById_NullRequest_Test()
        {
            PersonResponse? expected = await _personsService.GetPersonById(null);

            Assert.Null(expected);
        }
        #endregion

        #region GetPersonsBy

        [Fact] 
        public async Task GetPersonsBy_EmptyInput_Test()
        {
            //the list should end up with a PersonResponse from each add request above
            List<PersonResponse> expected = await _personsService.GetPersonsList();

            //by this time the expected list should have all the added persons
            //and the actual should have all persons as well since the property search fields were empty
            List<PersonResponse> actual = await _personsService.GetPersonsBy("","");

            foreach (PersonResponse person in expected)
            {
                Assert.Contains(person, actual);
            }
        }

        [Fact]
        public async Task GetPersonsBy_NameInput_Test()
        {
            //and the actual should have only one, Zappa
            List<PersonResponse> actual = await _personsService.GetPersonsBy(nameof(Person.Name), "Zap");

            //assert that the actual response list result is contained in expected
            //since the actual is the one being tested anyways, no need to check every entry in expected
            foreach (PersonResponse person in actual)
            {
                //asserting that whichever person in expected that the name matches 
                //the case insensitive partial string, is also contained in the expected simulated list
                Assert.True(person.Name.Contains("zap", StringComparison.OrdinalIgnoreCase)); //expected would be a database perhaps
            }
        }
        #endregion

        #region SortPersons
        [Fact]
        public async Task SortPersons_ValidInput_Test()
        {
            CountryAddRequest countryAddRequest1 = new CountryAddRequest() { Name = "Mongolia" };
            CountryAddRequest countryAddRequest2 = new CountryAddRequest() { Name = "Uzbekistan" };
            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryAddRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(countryAddRequest2);

            var unsorted = new List<PersonResponse>()
            {
                new PersonResponse() {Name = "Borat", Email = "email1@format.com", CountryId = countryResponse1.CountryId},
                new PersonResponse() {Name = "Zappa", Email = "email2@format.com", CountryId = countryResponse2.CountryId},
                new PersonResponse() {Name = "Stern", Email = "email3@format.com", CountryId = countryResponse1.CountryId},
                new PersonResponse() {Name = "Bean", Email = "email4@format.com", CountryId = countryResponse2.CountryId}
            };

            //first the unsorted list is passed to the test method,
            //actual is the list sorted by property in given sortOrder
            List<PersonResponse> sorted = await _personsService.SortPersons(unsorted, nameof(Person.Name), SortOrderEnum.Descending);
            
            //now sorting the unsorted list via another method for example brevity
            //the test could be invested to test Ascending and by other propertyNames as well
            unsorted = unsorted.OrderByDescending(person => person.Name).ToList();

            //now both lists should have same order
            for (int i = 0; i < unsorted.Count; i++)
            {
                //if lists are not sorted equally, the objects in target index
                //will be different and Assert will fail
                Assert.Equal(unsorted[i], sorted[i]);
            }
        }
        #endregion

        #region UpdatePerson
        [Fact]
        public async Task UpdatePerson_NullRequest_Test()
        {
            PersonUpdateRequest? personUpdateRequest = null;

            //aserting that a null exception is thrown if request is null
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _personsService.UpdatePerson(personUpdateRequest);
            });
        }

        [Fact]
        public async Task UpdatePerson_InvalidPersonId_Test()
        {
            //even though this request will have a required guid, its not a prexisting one in the list
            PersonUpdateRequest? personUpdateRequest = new PersonUpdateRequest()
            {
                PersonId = Guid.NewGuid(),
            };

            //aserting that an argument exception is thrown if request has invalid guid
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _personsService.UpdatePerson(personUpdateRequest);
            });
        }

        [Fact]
        public async Task UpdatePerson_ValidInput_Test()
        {
            CountryAddRequest countryAddRequest1 = new CountryAddRequest() { Name = "Monrovia" };
            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryAddRequest1);

            var AddRequest = new PersonAddRequest() { Name = "bumstead", Email = "email1@format.com", CountryId = countryResponse1.CountryId };

            //the list should end up with a PersonResponse list of above simulated list
            //expected serves as simulated preexisting list of persons
            PersonResponse expected = await _personsService.AddPerson(AddRequest);

            //create add request, note that this is a very simple implementation with
            //next to no safety checks, all of updated persons properties simply directly replaced
            //Update requires the target personId, imagine that only an authenticated logged in user can have this
            PersonUpdateRequest personUpdateRequest = expected.ToPersonUpdateRequest(); //taking first in simulated list
            personUpdateRequest.Email = "email44@format.com";
            personUpdateRequest.Name = "Billy Bob";

            //and the actual will contain the PersonResponse from details of updated person
            //note that next to no validation is implemented here, proper model binding and validation would be needed
            PersonResponse actual = await _personsService.UpdatePerson(personUpdateRequest);

            PersonResponse postUpdatePerson = await _personsService.GetPersonById(actual.PersonId);

            //aserting that the fetched person in preexisting list is found and has same personid
            //as the response to the update person, indicating the actual Person Domain instance was updated
            //since at least one of the rest of the values are different/updated
            //this obviously is very crude, each relevant field would be validated between fetched from list and the update response
            Assert.True(
                actual.PersonId == postUpdatePerson.PersonId &&
                (actual == postUpdatePerson) == false
                );
        }

        [Fact]
        public async Task UpdatePerson_ValidPersonIdInvalidInput_Test()
        {
            CountryAddRequest countryAddRequest1 = new CountryAddRequest() { Name = "Mongolia" };
            CountryAddRequest countryAddRequest2 = new CountryAddRequest() { Name = "Uzbekistan" };
            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryAddRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(countryAddRequest2);

            var addRequests = new List<PersonAddRequest>()
            {
                new PersonAddRequest() {Name = "Borat", Email = "email1@format.com", CountryId = countryResponse1.CountryId},
                new PersonAddRequest() {Name = "Zappa", Email = "email2@format.com", CountryId = countryResponse2.CountryId},
                new PersonAddRequest() {Name = "Stern", Email = "email3@format.com", CountryId = countryResponse1.CountryId},
                new PersonAddRequest() {Name = "Bean", Email = "email4@format.com", CountryId = countryResponse2.CountryId}
            };

            //the list should end up with a PersonResponse list of above simulated list
            //expected serves as simulated preexisting list of persons
            PersonResponse expected = new();

            //create add request, note that this is a very simple implementation with
            //next to no safety checks, all of updated persons properties simply directly replaced
            //Update requires the target personId, imagine that only an authenticated logged in user can have this
            PersonUpdateRequest personUpdateRequest = expected.ToPersonUpdateRequest(); //taking first in simulated list

            //the different from the test above and this one is the property value validation is being tested here
            //since Name is a [Required] field, even though the personId is of a valid person in the list
            //UpdatePerson should catch and rethrow the validation error thrown from PersonUpdateRequest.cs
            personUpdateRequest.Name = null;

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _personsService.UpdatePerson(personUpdateRequest);
            });
        }
        #endregion

        #region DeletePerson
        [Fact]
        public async Task DeletePerson_NullPersonId_Test()
        {
            PersonDeleteRequest? personDeleteRequest = null;
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _personsService.DeletePerson(personDeleteRequest);
            });
        }

        //add test for valid GUID but not found
        [Fact]
        public async Task DeletePerson_ValidGuidPersonNotFound_Test()
        {
            //note now the person data is only transported with DTOs, never the Domain Person instance this structure facilitates secure transport of data without affecting or compromising core data, DTOs could be encripted and transfered selectively with stricter model validation than demonstrated here, encrypted with secure headers, etc
            PersonDeleteRequest? personDeleteRequest = new PersonDeleteRequest() { PersonId = Guid.NewGuid()};

            //note that the Argument exception is thrown by the model validation call in DeletePerson so the exception is originating from the data annotation validations inside PersonDeleteRequest.cs directly
            
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _personsService.DeletePerson(personDeleteRequest);
            });
        }

        [Fact]
        public async Task DeletePerson_ValidRequest_Test()
        {
            List<PersonResponse> temp = await _personsService.GetPersonsBy("Name", "Zappa");
            PersonDeleteRequest personDeleteRequest = temp.FirstOrDefault().ToPersonDeleteRequest();

            Assert.True(await _personsService.DeletePerson(personDeleteRequest));
        }
        #endregion
    }
}
