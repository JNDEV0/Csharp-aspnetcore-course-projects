using System.ComponentModel.DataAnnotations.Schema;

namespace Entities
{
    //the Country class is considered part of the Domain model, that is, the data model in use
    //within the "domain" of the app, here we can expect to have a CountryID
    //note that the associated DTO wrapper class CountryAddRequest.cs does not have a Guid,
    //because it is meant to wrap the incoming request, which will not have a Guid generated yet
    //DTO objects wrap this data through transfers and validations first to ensure
    //an instance of this class is valid by time it is instantiated
    public class Person
    {
        //however many other properties are added here, they would follow the same logic
        //of expanding the class with optionally null/default fields. Why? 
        //depending on the incoming request, which may be reused in multiple front end forms
        //for example user registration only asks for name, later in profile page user enters DoB, Email, etc
        //so the same entity related DTO add request should have a wrapper class with the various fields
        public Guid PersonId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public DateTime? Dob { get; set; }
        public string? Gender { get; set; } //note that this field is string in Person but GenderOptions enum in request DTO wrappers
        public Guid? CountryId { get; set; }
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }
        public string? CountryName { get; set; }

        //there are multiple ways to stipulate the table relations, here in the CustomDbContext OnModelBuilder the Person entity is acessed and has the relation directly set using a "navigation property" of person, that has a Country type associated with it, so many persons have a CountryId key relation, and the Person class has the Country property. this is used by the database to identify the "foreignKey" of the other related object, so a Country having a ICollection<Person> can have this navigation property Included() to fetch the related instances of Person for that Country

        //initially this class had CountryId stored, which was used to interface with CountriesService to retrieve the countryname, then the countryname was added to this Person model, eventually a relational navigation property is added, Country. the Country class has the CountryName and CountryId already as its properties, so adding the relational property allows retrieving of those values directly from Country class rather than storing it in the Person class as well. the relational table in database provider will use the Country property and its [ForeignKey] annotation, to identify the Country object in the related table and populate it when the Person object is retrieved, if Include() is added, there are several ways to stipulate this relation, nullable so it will be assigned a value at runtime.
        //see PersonsService GetAllPersons() method call, where the CustomDbContext is calling the Persons collection, and Include("Country") the "foreign key" property in the Person object, that EFCore will pass on to database provider to populate the Country object from related table

        //instead of explicitly pointing out the relation of foreignkey CountryId to the Country Property of the Person though onModelBuilder,
        //the [ForeignKey("KeyOfPropertyOfOtherTable")] is annotated at Country in this example because the Person.CountryId is the key for the related Country object, efcore will fetch from database Country objects by Person.CountryId and populate it to the Country navigation property, remembering that the name given at _dbContext.Database.Include("x") where x==the PROPERTY NAME of the model class's navigation property here Country, then the foreignkey annotation will point to the Person.CountryId for the target key to search for and retrieve the related object for the particular Person instance, the child object here is called a "navigation property" as it stipulates the object relation in the parent object for easy access on demand, rather than having to join multiple separate tables manually in the Sql Query
        [ForeignKey("CountryId")]
        public Country Country { get; set; }

        //this example property below is used to showcase how migrations are used to update the database provider.
        //the property below was added after this domain model class was added, so the database does not have the property below as well, and so a migration is performed. the migration detects the new property and creates the needed operations to update the database model. the Persons collection should have a new column named ExampleChangedProperty after migrating to integrate this addition
        //note also that the stored procedure that retrieves Person data from database will fail, since it does not consider the newly added property in the hardcoded operation of the stored procedure, which needs to be updated as well in A SEPARATE NEW migration after updating this new property to database
        //this explanation is to showcase the process of runtime data to database relation of data and expectations
        //public string? ExampleChangedProperty { get; set; }
        //the ExampleChangedProperty was renamed in the database as TaxId, see migrations history, so now in sourcecode it needs to be updated as well
        public string? TaxId { get; set; }

        public Person()
        {
            PersonId = Guid.NewGuid();
        }
    }
}
