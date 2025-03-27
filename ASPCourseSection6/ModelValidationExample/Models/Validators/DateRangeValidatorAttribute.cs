using System.ComponentModel.DataAnnotations;
using System.Reflection; //reflection is a process of accessing data of objects indirectly

namespace ModelValidationExample.Models.Validators
{
    public class DateRangeValidatorAttribute : ValidationAttribute
    {
        internal string TargetPropertyName { get; set; }

        //the target property to compare the one that has this tag is received with constructor
        public DateRangeValidatorAttribute(string targetPropertyName)
        {
            TargetPropertyName = targetPropertyName;
        }

        //as part of model binding this attribute's custom validator IsValid is called, and the actual Model, in this case Person.cs, is instantiated within the validationContext
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            //if no value is passed to the field being checked, in this project example, the ToDate field of Person.cs model
            if (value == null) return null;
         
            //being that ToDate is not null, casting to DateTime to perform comparison
            DateTime toDate = Convert.ToDateTime(value);

            //here object type will actually be Person, and by naming the target property's location in memory from GetProperty
            PropertyInfo? targetProperty = validationContext.ObjectType.GetProperty(TargetPropertyName);
            
            //if target property is null, stop here
            if (targetProperty == null) return null;

            //however if valid it is just a reference to the target property of that given class type, not the property of the instantiated object, so using projection we get the actual value of the property, of the given ObjectInstance, and convert to DateTime 
            DateTime fromDate = Convert.ToDateTime(targetProperty.GetValue(validationContext.ObjectInstance)); //possibly unsafe casting, implement error handling
    
            //now finally with both to and from date in memory, verify
            if (fromDate > toDate)
            {
                //ErrorMessage was already set at the attribute constructor above the property name in 
                return new ValidationResult(ErrorMessage, new string[] { TargetPropertyName, validationContext.MemberName });
            }
            //if all checks pass, return success
            return ValidationResult.Success;
        }
    }
}
