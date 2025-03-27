
using System.ComponentModel.DataAnnotations;

namespace Entities
{
    //the Country class is considered part of the Domain model, that is, the data model in use
    //within the "domain" of the app, here we can expect to have a CountryID
    //note that the associated DTO wrapper class CountryAddRequest.cs does not have a Guid,
    //because it is meant to wrap the incoming request, which will not have a Guid generated yet
    public class Country
    {
        [Key]
        public Guid CountryId { get; set; }
        public string? Name { get; set; }

        //the Persons collection in Country is a navigation property that relates Persons to this Country, this is being set on PersonsDbContext OnModelBuilder note this runtime collection will be null by default when retrieving a Country object, and the Persons related collection can be retrieved later
        //this collection of Persons, is made virtual so can be overriden later
        //there are multiple ways to stipulate the table relations, here in the PersonsDbContext OnModelBuilder the Person entity is acessed and has the relation directly set using a "navigation property" of person, that has a Country type associated with it, so many persons have a CountryId key relation, and the Person class has the Country property. this is used by the database to identify the "foreignKey" of the other related object, so a Country having a ICollection<Person> can have this navigation property Included() to fetch/populate the related instances of Person for that given Country
        public virtual ICollection<Person>? Persons { get; set; }
        
        public Country(string name)
        { 
            CountryId = Guid.NewGuid();
            Name = name;
        }
    }
}
