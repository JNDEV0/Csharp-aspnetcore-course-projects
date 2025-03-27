using Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ServiceContracts.Enums;
using System.IO;
using System.Runtime.CompilerServices;

namespace ServiceContracts.DTO
{
    /// <summary>
    /// DTO class is used as return type for PersonService methods, typically after an incoming request
    /// </summary>
    public class PersonResponse
    {
        public Guid PersonId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public DateTime? Dob { get; set; }
        public GenderOptions? Gender { get; set; } //note that this field is string in Person but GenderOptions enum in request DTO wrappers
        public Guid? CountryId { get; set; }
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }
        public string? CountryName {  get; set; }

        public override bool Equals(object? obj)
        {
            //the default Equals implementation compares two objects which even if they have 
            //the exact same exact data its two references and so not the same
            //return this == obj;

            //so here we add validation to ensure that the generic object type is valid
            if (obj is null) return false;
            if (obj.GetType() != typeof(PersonResponse)) return false;

            //then cast it and compare the field values directly
            PersonResponse other = (PersonResponse)obj;

            //if any of the object fields are different, will return false
            return
                this.PersonId == other.PersonId &&
                this.Name == other.Name &&
                this.Email == other.Email &&
                this.Dob == other.Dob &&
                this.Gender == other.Gender &&
                this.CountryId == other.CountryId &&
                this.Address == other.Address &&
                this.ReceiveNewsLetters == other.ReceiveNewsLetters &&
                this.CountryName == other.CountryName;
        }

        public PersonAddRequest ToPersonAddRequest()
        {
            return new PersonAddRequest()
            {
                //note that no Guid is passed, because the Person() constructor generates the guid on instantiation
                PersonId = this.PersonId,
                Name = this.Name,
                Email = this.Email,
                Dob = this.Dob,
                Gender = this.Gender is not null ? this.Gender : GenderOptions.Other,
                CountryId = this.CountryId,
                Address = this.Address,
                ReceiveNewsLetters = this.ReceiveNewsLetters,
            };
        }

        //note that this method is belongs to PersonResponse instance
        public PersonUpdateRequest ToPersonUpdateRequest()
        {
            return new PersonUpdateRequest()
            {
                PersonId = this.PersonId,
                Name = this.Name,
                Email = this.Email,
                Dob = this.Dob,
                //the person has string gender, here we parse the Enum option from the string
                //since the PersonResponse has the GenderOptions enum instead of string
                Gender = this.Gender,
                CountryId = this.CountryId,
                Address = this.Address,
                ReceiveNewsLetters = this.ReceiveNewsLetters
            };
        }

        public PersonDeleteRequest ToPersonDeleteRequest()
        {
            return new PersonDeleteRequest()
            {
                PersonId = this.PersonId
            };
        }
    }

    
    /// <summary>
    /// Utility class used to convert Person class into PersonResponse using static extension method
    /// </summary>
    public static class PersonUtils
    {
        //an extension class is a static class that it's methods have (this Object object) parameters
        //the idea is IncludeThisMethod(in Classtype class)
        //this method can then be called from PersonInstance.ToPersonResponse() in this case
        //it can be confusing to understand, but the parameter dictates the class that receives the extension method
        //so this in fact is not PersonResponse.ToPersonResponse() but rather Person.ToPersonResponse()
        //even though the code is in the same file and namespace as PersonResponse
        public static PersonResponse ToPersonResponse(this Person person)
        {
            //ToPersonResponse used the now obsolete methods for retrieving country data from CountryService's runtime mock data, which is no longer needed as the database now has the nested Country relation to each Person, so fetching the Country details to set it for PersonResponse can be done directly through the navigation relational property, that is called by the method that passes the person parameter to this one, so if Include() populates the Country nav property, the country data is retrieved from it directly instead of through CountryService
            return new PersonResponse()
            {
                PersonId = person.PersonId, 
                Name = person.Name,
                Email = person.Email,
                Dob = person.Dob,
                //the person has string gender, here we parse the Enum option from the string
                //since the PersonResponse has the GenderOptions enum instead of string
                Gender = Enum.Parse<GenderOptions>(person.Gender), 
                CountryId = person.Country?.CountryId,
                Address = person.Address,
                ReceiveNewsLetters = person.ReceiveNewsLetters,
                //once the nested related object is populated using Include(), its properties are not null as well and can be acessed, such as retrieving person.Country.CountryName. the point is this nested object value retrieval is not by default so first we check if ?null before setting the value, in case Include() is not called, this may be intentional since the related table Country data may not be needed.
                //this is stipulated in the DTO PersonResponse, that will take its values from the Person class, that has the [foreignKey] tags,
                //and the call to Include() or not is done at the PersonsService methods when accessing the database
                CountryName = person.Country?.Name
            };
        }

    }
}
