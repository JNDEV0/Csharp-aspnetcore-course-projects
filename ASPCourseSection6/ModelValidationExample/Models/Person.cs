using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ModelValidationExample.Models.Validators;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding; //used by [BindNever] attribute, to prevent binding of a property


//Model binding occurs in order:
//form fields, request body, route data, query string parameters
//typically form fields come from html fields, encoded as form-data or multipart-form-data, the request body can contain data usually in JSON XML or other ucstom format, route data that is identifying the route to send the call to actionMethod, and finally reviewing the URL?query string for any KVPs
//finally, after all validations restrictions, if data is valid, the actionMethod is executed with the processed request data into the instantiated Model object inside the actionMethod scope
namespace ModelValidationExample.Models
{
    //IValidatableObject can be implemented directly into the model class that will need validation, instead of creating a separate validatorAttribute class that would then have to be imported 
    //the point is a validatorAttribute is easily reusable in multiple models, while implementing IValidatableObject into the model direct skips the external implementation at the cost of reusability, perhaps its a very specialized validation only used once anyways
    //for IValidatableObject see Validate() method below
    public class Person : IValidatableObject
    {
        //Model Validation is done using [attributes] and property {get;set;} validation rules, such as calling an internal method that performs checks on the value, setting it to the valid number or -1 to signal invalid for example, since null values are best avoided
        //Failing validation of a field will still allow instantiation of the object in the actionMethod and set the field to null, AND model validation throws an internal error that can be checked to see that the validation failed using IsValid, Values and ErrorCount

        //this [Required] attribute will stipulate the property must have a value, cannot be null or empty
        //the ErrorMessage takes a string to associate with the error, in cause it occurs, and is then added to the ModelState.Values[x].ErrorMessage, and also will cause ModelState.IsValid to be false if the error triggers
        [Required(ErrorMessage = "The name can't be empty or null")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "{0} should be between {2} and {1} characters length")] //this StringLength property is a bit confusing. first maxChars, second MinChars, third errorMessage in case of failed validation. within the error message, the property name is {0}, minChar is {2} and maxchar is {1}.
        [RegularExpression("^[A-Za-z -.]*$", ErrorMessage = "{0} must be a-z, can have dot(.) space( ) and dash(-).")]
        public string? Name { get; set; }

        //a collection of values can be provided by the request, say for example on the request body a collection of search tags the user is looking for, to populate this field the KVPs would look like this:
        //tags[0]=shirts&tags[1]=pants&tags[2]=underwear and so on...
        //the VALUE of each list object will have the assigned value
        List<string?> tags { get; set; } = new List<string?>();

        //this YearAttribute is the custom validation class in Models/Validators
        //note that the number passed is a custom configured parameter and the ErrorMessage property being passed here as constructor parameters
        //the way YearAttribute has been configured, param1 will be the minimum year being set to validate against the incoming request's data, after being parsed into the Models.Person instance
        //the {0} will be a parameter that must be passed internally inside the custom validator's IsValid override, when it returns ValidationResult?
        //custom Validator class types can be reusable, this example could be configured to validate a Year in other contexts, like date of purchase, or find sales report for year x etc.
        [YearValidatorAttribute(1900, ErrorMessage = "Date of Birth should be after year {0}.")]
        //example: [YearAttribute(1900)] where no ErrorMessage is provided to the constructor parameters, the custom validator class must have its own default error message set internally, probably a field string
        public DateTime? DateOfBirth { get; set; }

        //the {0} signals the name of the property associated, here Email, its the only value, {1}, {2} etc is invalid
        //RegularExpression can be used as well as EmailAddress
        [EmailAddress(ErrorMessage = "The {0} can't be empty or null.")]
        [Display(Name = "e-mail")] //can change the way the property name is displayed
        [Required(ErrorMessage = "{0} is required")]
        public string? Email { get; set; }

        //[RegularExpression("^0-9$", ErrorMessage = "{0} must be digits.")] does the same as [Phone()]
        [Phone(ErrorMessage = "{0} must be valid.")]
        public int? Phone { get; set; }

        //here a [Range()] is set to a minimum and maximum deposit amount, along with error message that uses the property name, min and max values
        [Range(5, 3000, ErrorMessage = "{0} must be between ${2} and ${1}.")]
        public double? DepositAmount { get; set; }

        //not adding Required attribute will make the field optional, meaning if no value is provided for the field it will not through an error, unless [Required]
        [Required(ErrorMessage = "{0} cant be empty or null.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "{0} cant be empty or null.")]
        [Compare("Password", ErrorMessage = "{0} and {1} do not match")] //the first parameter is the name of the target property to compare to in this case comparing ConfirmPassword that has this tag with Password named field
        [Display(Name = "Confirm password")]
        public string? ConfirmPassword { get; set; }

        [ValidateNever] //excludes the field from any validation, even if there are other tags verifying validation on this field, useful for testing
        [BindNever] //binding is the task of selecting which properties of the data model will be "bound" or corelated to data incoming from the request. a field may have internal purposes where an external value should actually never be received from the request, so [BindNever] prevents malicious attempts of injecting values where it should not be received. see [Bind] in the actionMethod of the example route in HomeController.cs
        public string? UserTheme { get; set; }

        public DateTime? FromDate { get; set; }

        //the purpose of this custom validator is to check that FromDate is in the past, compared to ToDate
        //configured to receive the name of the target
        [DateRangeValidatorAttribute("FromDate", ErrorMessage = "FromDate must be before ToDate.")]
        public DateTime? ToDate { get; set; }

        public int? Age { get; set; }

        public override string ToString()
        {
            return $"N:{Name} E:{Email} P:{Phone}";
        }

        //Validate is from IValidatableObject interface, is called AFTER model binding of request data into Model instance, and BEFORE actionMethod logic execution
        //IMPORTANT validate only executes AFTER all route parameter validations, request parsing into request, model binding to identified actionMethod and instantiation AND completion of all model field attribute validations, including custom validationAttribute classes
        //Validate will only execute if the model binding IS SUCCESSFUL, meaning if any errors occur in model binding, Validate will never be called
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //note that yield keyword here allows the validation to return more than one ValidaitonResult
            //for example all the fields of the Person Model could be validated here one by one, each field with an error yield returning its own validationResult
            //these are then converted into an IEnumerable<ValidationResult> returned as IEnumerable collection
            if (DateOfBirth.HasValue is false && Age.HasValue is false)
            {
                yield return new ValidationResult("validation error, missing fields", new[] { nameof(Age), nameof(DateOfBirth)});
            }

            //note that Validate() does not need to always return a value, it will only return a ValidationResult with an error in this case, if both fields are missing a value after model binding
            //Validate is a quick way to validate multiple properties, without separate implementations
        }
    }
}
