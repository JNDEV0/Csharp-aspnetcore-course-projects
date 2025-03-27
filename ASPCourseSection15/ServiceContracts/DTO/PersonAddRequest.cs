//DTO stands for Data Transfer Object, a model wrapper class used to transport the target data
using Entities;
using ServiceContracts.Enums;
using System.ComponentModel.DataAnnotations; //data annotations are related to model binding validation tags

namespace ServiceContracts.DTO
{
    public class PersonAddRequest
    {
        //note the add request normally does not have a personId, as it is generated once the person is actually added
        //via person service class, however there can be reverse utility in reverting a person response back to a add request
        //in which case a PersonId is provided
        public Guid? PersonId { get; set; }

        //note that the Person domain class has a Guid, and here it is not expected since this is a DTO class
        [Required(ErrorMessage = "Name is required")]
        [Length(2, 100)]
        public string? Name { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email should be a valid email format")]
        //by assigning a datatype, this property will have its type="" attribute on a tag helper assigned to the given type, see AddNewPerson.cshtml view
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
        //other types of validation could be added, such as Regex, datetime, etc
        //but not that model binding validation this way may limit the reusability of the DTO class
        //for example if all fields here are required, this DTO would not be useful for a simple user registration
        //form that only asks for name and email, since this DTO would require all fields
        [DataType(DataType.DateTime)]
        public DateTime? Dob { get; set; }
        public GenderOptions? Gender { get; set; } //note that this field is string in Person but GenderOptions enum in request DTO wrappers
        public Guid? CountryId { get; set; }
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }

        public PersonAddRequest() { }

        //to facilitate retrieving the Person object from PersonAddRequest
        //note that Person.cs is in Entities project in this solution, reference has been added
        //the Person class generates a Guid via constructor, by time this method is called
        //its assumed the input is safe and validated elsewhere
        public Person ToPerson()
        {
            return new Person()
            {
                //note that no Guid is passed, because the Person() constructor generates the guid on instantiation
                Name = this.Name,
                Email = this.Email,
                Dob = this.Dob,
                Gender = this.Gender is not null ? this.Gender.ToString() : "Other",
                CountryId = this.CountryId,
                Address = this.Address,
                ReceiveNewsLetters = this.ReceiveNewsLetters,
            };
        }
    }
}
