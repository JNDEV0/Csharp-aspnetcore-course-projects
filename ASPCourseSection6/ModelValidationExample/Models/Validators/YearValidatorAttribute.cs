using System.ComponentModel.DataAnnotations;

//when the Custom Validator is used as an Model's property validation attributes, if that Model is then assigned as an actionMethod's parameter, the IsValid method that is overriden by this Custom Validator will be called as part of the Model binding validation on a request that has been routed to the actionMethod that receives the Model, for which the validation needs to take place
namespace ModelValidationExample.Models.Validators
{
    //Custom Validator class can be named anything the 'Attribute' being part of the name is not required, merely descriptive, however must inherit ValidationAttribute
    //The predefined validation classes, have common features of accepting parameters and optional ErrorMessage and default errorMsg, here we achieve the same in this custom Validation class
    //The point of a Custom Validator class is to return true or false+errorMsg depending on ValidationResult
    public class YearValidatorAttribute : ValidationAttribute
    {
        //constructors, default fields and errorMsg are optional, one can simply override the IsValid method inherited from ValidationAttribute
        internal int MinYear { get; set; } = 2000; //by default parameterless instantion will have MinYear == 2000
        internal string ErrorMsg { get; set; } = "Default Error: Date of Birth should be after year {0}.";

        public YearValidatorAttribute() { } // parameterless

        //in order to configure custom parameters for the validator being assigned to a model property as an attribute, this custom validator needs a constructor and properties to store the instantiation provided parameters
        public YearValidatorAttribute(int minYear) 
        { 
            MinYear = minYear; // if minYear is provided, will override MinYear
        }

        //IsValid will be called on this custom validation method as part of the model bound validation, so any custom validation logic should be inside or valled by IsValid
        //when this custom validator is associated as an attribute with a model's field, when its associated action method is called
        //returning null means  IsValid will be false, which fails the validation, else returning .Success which is == true, will pass validation
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value == null)
            return null;

            //the value object should receive the data to validate, can be cast into the right class type if needed 
            //in this example, this validator is being used to check that date of birth's year, so yyyy-mm-dd format usually, various formats can be instantiated as DateTime.
            //the info in the request may be a query, form data, routedata, etc
            DateTime date = (DateTime)value; //possible null exception hard cast
            if (date.Year >= 2000)
            {
                //if the ErrorMessage has not already been set through the validator constructor, then set it to the internal default msg  field
                //another way would be to return ValidationResult(string.Format(ErrorMessage ?? ErrorMsg, MinYear)) where ?? will chose ErrorMsg if ErrorMessage is null
                if (string.IsNullOrEmpty(ErrorMessage))
                    ErrorMessage = ErrorMsg;


                //when the validationResult returns with an error message, the ModelState.Values[x].Errors[x].ErrorMessage will store the error message thrown for this specific validator
                //the error message is passed here or by calling ErrorMessage which will be the predefined error string stipulated at the attribute constructor, in this case the actual text of the error message is at Person.cs's [YearAttribute(ErrorMessage = "message")] above the Person.DateOfBirth
                //return new ValidationResult("error: min is 2000");
                //here the ErrorMessage and the variable MinYear are being formated into one string, where {0} in the errorMessage will be replaced by MinYear by string.Format
                return new ValidationResult(string.Format(ErrorMessage, MinYear)); // return "Date of Birth should be after year 2000." by default
            }
            else
            {
                //the .Success property will be true by default if the above validation fails
                return ValidationResult.Success;
            }
        }
    }
}
