﻿using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDxUnitExample.Controllers
{
    //sets the entire controller routes to start with given route attribute/
    //localhost:4255/persons/x where x is whatever other routes are applied to action methods in the controller class
    //[Route("[controller]")] //takes the name of controller class as base parameter
    [Route("persons")]
    public class HomeController : Controller
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;

        //runtime mockupData example
        //public HomeController(IPersonsService personsService, ICountriesService countriesService, IMockDataUtil mockDataUtil)
        //{
        //    List<PersonResponse>? mockPersonsResponseList;
        //    List<CountryResponse>? mockCountriesResponseList;
        //    var mockData = mockDataUtil.FetchMockData(out mockPersonsResponseList, out mockCountriesResponseList);

        //    if (mockData == null)
        //    {
        //        foreach (var person in mockPersonsResponseList)
        //        {
        //            mockDataUtil.mockPersonsService.AddPerson(person.ToPersonAddRequest());
        //        }
        //        foreach (var country in mockCountriesResponseList)
        //        {
        //            mockDataUtil.mockCountriesService.AddCountry(country.ToCountryAddRequest());
        //        }
        //    }

        //    _personsService = mockDataUtil.mockPersonsService;
        //    _countriesService = mockDataUtil.mockCountriesService;
        //}

        public HomeController(IPersonsService personsService, ICountriesService countriesService)
        {
            _personsService = personsService;
            _countriesService = countriesService;
        }

        //note about async/await
        //initially the service classes were implemented to use a local memory mockData collection
        //and so there was no external Input/Output services that had to be asynchronously accessed and waited on
        //so returning IActionResult is fine in those cases, where perhaps the server is intended to handle local memory data
        //now the external database provider is implemented through dbContext inside the CustomService classes above
        //and the methods accessed through the ICustomService interface also return Task<T> to handle the async aspect of accessing the external database
        //all methods in CustomService classes were converted into async returning Task<T>, this also needs to be updated in the ICustomService interface in ServiceContracts.
        //this will cause issues in any method call that previously did not need to account for an async response, so Controllers, CustomServices etc anywhere the now async methods are called need to be updated to correctly use async/await
        //for a controller action method, test case etc to call the async methods from CustomService, ICustomService, the method calls need to also be async up the call stack so all actionMethods below are also async

        //note the parameters of this actionmethod are being used as a callback from the view request,
        //the View.cshtml provides a formatted form that takes <html> component attributes with "name" and sends them
        //as query parameters to the given route, so an actual request may look like:
        //website.com/persons/search?searchBy=Name&searchString=Zappa and so on note that these parameters are given some default values
        //to handle initial page requests with prefilled values on the form
        [Route("search")] //if route is /search then itd be website.com/search as the extra / overrides controller parent class
        //[Route("[action]")] //takes name of action method, this would be website.com/persons/index
        [Route("/")]
        public async Task<IActionResult> Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.Name), SortOrderEnum sortOrder = SortOrderEnum.Ascending)
        {
            //note that the _viewImports.cshtml of the views folder brings in various dependencies,
            //but not the domain level models, only DTOs and selection enums, this matters to keep separation
            //between unreliable user input and reliable domain level data

            //this example SearchFields dicitonary is supplied to the view to populate the searchBy functionality of the view
            //this like other data supplied by controller to view, should be retrieved through a Dependency injection service
            //added here for example brevity, the point is model property is to be associated with the string search term
            ViewBag.SearchFields = new Dictionary<string, string>()
            {
                //{ nameof(PersonResponse.PersonId), ""}, 
                { nameof(PersonResponse.Name), "Person Name"},
                { nameof(PersonResponse.Email), "Email Address"},
                { nameof(PersonResponse.Dob), "Date of birth"},
                { nameof(PersonResponse.Gender), "Gender"},
                { nameof(PersonResponse.CountryName), "Country"},
                { nameof(PersonResponse.Address), "Address"}

            };
            //foreach (PropertyInfo propertyInfo in PersonResponse.GetType().GetProperties())
            //{
            //    if (propertyInfo.GetValue() is not null)
            //    // do stuff here
            //}
            //ViewBag.Persons = _personsService.GetPersonsList();

            //will either be all persons or filtered by property name and value
            List<PersonResponse> personsList = await _personsService.GetPersonsBy(searchBy, searchString);

            //NORMALLY there would not be a callback to the same actionMethod the way we implement in this example to not rely on viewbag to persist relevant data, instead there would can be a route that receives and validates request model data from URL route parsing see previous projects
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchString = searchString;

            List<PersonResponse> sortedPersons = await _personsService.SortPersons(personsList, sortBy, sortOrder);
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder;


            //passing the persons list as model data
            //note that the actual view is @model bound to IEnumerable<> since list inherits it
            return View(sortedPersons);
        }

        [Route("add-new")]
        [HttpGet] //limits the route to only receive GET requests to fetch the add person page
        public async Task<IActionResult> AddNewPerson()
        {
            ViewBag.Countries = await _countriesService.GetCountriesList();
            return View();
        }

        [Route("add-new")]
        [HttpPost] //limits the route to only receive POST requests to actually add a person
        public async Task<IActionResult> AddNewPerson(PersonAddRequest addRequest)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(exceptionInstance => exceptionInstance.Errors).Select(error => error.ErrorMessage).ToList();
                return View();
            }
            
            PersonResponse result = await _personsService.AddPerson(addRequest);
            ViewBag.Success = $"{result.Name} added to list";
            return View();
        }

        [Route("[action]/{personId}")]
        [HttpGet]
        public async Task<IActionResult> EditPerson(Guid personId)
        {
            //the edit person route receives the personId, personservice retrieves person PersonResponse to update request fetched countries passed via viewbag to edit view, that displays existing values from retrieved person when edit link is clicked for a person on list search view, a [HttpGet] route is activated by the request to edit, when the edit submit button is clicked on the edit page, routed to [HttpPost] tagged Edit actionMethod below
            PersonResponse? targetPerson = await _personsService.GetPersonById(personId);
            if (targetPerson is null)
            {
                Redirect("/");
            }

            ViewBag.Countries = await _countriesService.GetCountriesList();
            return View(targetPerson.ToPersonUpdateRequest());
        }

        [Route("[action]/{personId}")]
        [HttpPost] 
        public async Task<IActionResult> EditPerson(PersonUpdateRequest updateRequest)
        {
            ViewBag.Countries = await _countriesService.GetCountriesList();
            //note the ModelState.IsValid will be false if any of the incoming values fail the PersonUpdateRequest model validation rules defined by validation tags
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(exceptionInstance => exceptionInstance.Errors).Select(error => error.ErrorMessage).ToList();
                return View(updateRequest);
            }
            PersonResponse? updateConfirmation = await _personsService.UpdatePerson(updateRequest);
            ViewBag.Success = updateConfirmation is not null ? $"{updateRequest.Name} updated." : "failed to update internal error";
            return View(updateRequest);
        }

        [Route("[action]/{personId}")]
        [HttpGet]
        public async Task<IActionResult> DeletePerson(Guid personId) 
        {
            PersonResponse? targetPerson = await _personsService.GetPersonById(personId);
            if (targetPerson is null) 
            { 
                Redirect("/");
            }
            PersonDeleteRequest deleteRequest = targetPerson.ToPersonDeleteRequest();
            await _personsService.DeletePerson(deleteRequest);
            return View(deleteRequest);
        }

        [Route("[action]/{personId}")]
        [HttpPost]
        public async Task<IActionResult> DeletePerson(PersonDeleteRequest deleteRequest) 
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(exceptionInstance => exceptionInstance.Errors).Select(error => error.ErrorMessage).ToList();
                return View(deleteRequest);
            }
            bool deleted = await _personsService.DeletePerson(deleteRequest);
            ViewBag.Success = deleted ? $"{deleteRequest.PersonId} deleted." : "failed to delete internal error";
            return View(deleteRequest);
        }
    }
}
