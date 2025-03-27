using ServiceContracts.DTO;
using ServiceContracts.Enums;
using ServiceContracts;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace Services.Utils
{
    //this MockDataUtility class was used when the system was initially setup to use local memory lists
    //eventually all services were converted to async/await and external Input/Output database provider through DbContext
    //so the implementation of this utility class is no longer needed
    //public class MockDataUtil : IMockDataUtil
    //{
    //    List<PersonResponse> mockPersons;
    //    List<CountryResponse> mockCountries;
    //    public IPersonsService mockPersonsService { get; set; }
    //    public ICountriesService mockCountriesService { get; set; }
    //    public (List<PersonResponse>, List<CountryResponse>)? MockData { get; set; }

    //    public MockDataUtil()
    //    {
    //        mockPersons = new List<PersonResponse>();
    //        mockCountries = new List<CountryResponse>();
    //        mockCountriesService = new CountriesService();

    //        //previously using an empty mockup customService class to initialize some runtime collection seed data,
    //        //mockPersonsService = new PersonsService(mockCountriesService);

    //        //eventually implemented CustomDbContext to interface with database provider,
    //        //which requires the DbContextOptionsBuilder provided Options to set the database provider, sql, mongodb etc
    //        mockPersonsService = new PersonsService(new PersonsDbContext(new DbContextOptionsBuilder<PersonsDbContext>().Options));
    //    }

    //    public (List<PersonResponse>, List<CountryResponse>)? FetchMockData(out List<PersonResponse>? personsList, out List<CountryResponse>? countriesList)
    //    {
    //        if (mockCountriesService.GetCountriesList().Count == 0)
    //        {
    //            CountryAddRequest countryAddRequest1 = new CountryAddRequest() { Name = "Mongolia" };
    //            CountryAddRequest countryAddRequest2 = new CountryAddRequest() { Name = "Uzbekistan" };

    //            mockCountries.Add(mockCountriesService.AddCountry(countryAddRequest1));
    //            mockCountries.Add(mockCountriesService.AddCountry(countryAddRequest2));
    //        }

    //        if (mockPersonsService.GetPersonsList().Count == 0)
    //        {
    //            var personAddRequests = new List<PersonAddRequest>()
    //                {
    //                new PersonAddRequest() {Name = "Borat", Email = "email1@format.com", Dob = DateTime.Parse("3/11/1987"), Gender = GenderOptions.Other, CountryId = mockCountries[0].CountryId, Address = "123 trueman st", ReceiveNewsLetters = true},
    //                new PersonAddRequest() {Name = "Zappa", Email = "email2@format.com", Dob = DateTime.Parse("6/8/1982"), Gender = GenderOptions.Male, CountryId = mockCountries[0].CountryId, Address = "44 electric way", ReceiveNewsLetters = true},
    //                new PersonAddRequest() {Name = "Stern", Email = "email3@format.com", Dob = DateTime.Parse("7/12/1984"), Gender = GenderOptions.Female, CountryId = mockCountries[1].CountryId, Address = "32 scolding blvd", ReceiveNewsLetters = false},
    //                new PersonAddRequest() {Name = "Bean", Email = "email4@format.com", Dob = DateTime.Parse("1/10/1981"), Gender = GenderOptions.Male, CountryId = mockCountries[1].CountryId, Address = "21 juanito dr", ReceiveNewsLetters = true}
    //                };

    //            //the list should end up with a PersonResponse list of above simulated list
    //            //expected serves as simulated preexisting list of persons
    //            for (int i = 0; i < personAddRequests.Count; i++)
    //            {
    //                mockPersons.Add(mockPersonsService.AddPerson(personAddRequests[i]));
    //            }
    //        }

    //        personsList = mockPersonsService.GetPersonsList();
    //        countriesList = mockCountriesService.GetCountriesList();
    //        MockData = (mockPersons, mockCountries);

    //        return MockData;
    //    }
    //}
}
