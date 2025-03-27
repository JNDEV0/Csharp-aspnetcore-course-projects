//this interface class initially utilized runtime memory mockup data, so the classes did not need to be asynchronous, since the application did not communicate externally, now the methods are being converted to async/await, and so the Interface needs to return Task<T> where T is the otherwise non-async type still

using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts
{
    //will be useful for dependency injection of the services into the app builder
    //represents business logic for manipulating country service instance through Inversion of Control container
    public interface IPersonsService
    {
        /// <summary>
        /// Add person to the list of countries
        /// </summary>
        /// <param name="personAddRequest">request of country to be added</param>
        /// <returns>contains Guid and Name, confirmation of operation</returns>
        //public PersonResponse AddPerson(PersonAddRequest? personAddRequest);
        public Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest);

        /// <summary>
        /// Get list of all persons in PersonResponse DTOs
        /// </summary>
        /// <returns></returns>
        public Task<List<PersonResponse>> GetPersonsList();

        /// <summary>
        /// attempt to get a person's details through PersonResponse DTO
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        public Task<PersonResponse?> GetPersonById(Guid? personId);

        /// <summary>
        /// returns all Persons with matching given propertyName and propertyValue
        /// </summary>
        /// <param name="propertyName">field propertyName to search by</param>
        /// <param name="propertyValue">the expected value</param>
        /// <returns></returns>
        public Task<List<PersonResponse>> GetPersonsBy(string propertyName, string? propertyValue);

        /// <summary>
        /// sort the given list by property name and direction
        /// </summary>
        /// <param name="unsortedList">list that needs sorting</param>
        /// <param name="sortByPropertyName">string name of property to sort by</param>
        /// <param name="sortDirection">ascending or descending</param>
        /// <returns></returns>
        public Task<List<PersonResponse>> SortPersons(List<PersonResponse> unsortedList, string sortByPropertyName, SortOrderEnum sortDirection);

        /// <summary>
        /// update the details of a given person
        /// </summary>
        /// <param name="personUpdateRequest">DTO for update person request</param>
        /// <returns></returns>
        public Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest);

        /// <summary>
        /// delete a person from the list
        /// </summary>
        /// <param name="personDeleteRequest">the DTO for person to be deleted</param>
        /// <returns></returns>
        public Task<bool> DeletePerson(PersonDeleteRequest? personDeleteRequest);
    }
}
