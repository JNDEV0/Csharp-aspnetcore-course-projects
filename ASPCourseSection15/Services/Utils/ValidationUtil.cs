using System.ComponentModel.DataAnnotations;

namespace Services.Utils
{
    //this utility class stores a static reusable method for data annotated model validation, see Person class and PersonAddRequest
    //to see the rules and call to this method
    public class ValidationUtil
    {
        internal static void ModelValidation(object obj)
        {
            //below are some simple validations directly in the target class, but there is a better way using model validation
            //the validation happens within the context of the target class's definition, using validation headers on properties
            ValidationContext valContext = new ValidationContext(obj);

            //the results will be stored in a list, which stores the validation result of each evaluated property ofcontext class
            List<ValidationResult> valResults = new List<ValidationResult>();

            //try to validate the 1st param object instance,
            //in the validation context of its class definition
            //store results in validationresult list
            //4th param stipulates to validate ALL types of validation rules, [Email], [Regex] etc, false or default will validate only [Required]
            bool isValid = Validator.TryValidateObject(obj, valContext, valResults, true);
            if (!isValid) //if false, there is at least one validation error
            {
                //if any validations fail, this codeblock will activate and return the first error message
                //could be implemented instead to return all error messages
                throw new ArgumentException(valResults.First().ErrorMessage);
            }
        }
    }
}
