using Microsoft.AspNetCore.Mvc.ModelBinding;

//custom model binders ARE NOT the right place to validate the incoming data, use the default Model.Property [attributes] and [customAttributeValidators], the custom model binder's purpose is to process the incoming data into the model bound data model object that will be accessible in the actionMethod the request has been routed to. ex: firstName and lastName query KVPs are converted into Person.Name's value.
namespace ModelValidationExample.Models.Binders
{
    //a custom model binder, is a way of implementing custom logic for the requestData to datamodel correlation of values.
    //for example a request may send firstName and lastName as 2 strings, but in the Person.cs model binding we wish to have simply Name property, so firstName and lastName need to be concatenated,
    //and in model binding this logic can be performed at the data binding step. generally custom mode binding is avoided using standard validation and custom validators
    //the name of this class is only descriptive, what matters is implementing IModelBinder
    public class PersonModelBinder : IModelBinder
    {
        //BindModelAsync will be called at modelbinding stage reading values of incoming request data processing them into values of the target data model that will be instantiated, in this case a Person class object
        //fields that are not assigned here to their requestData values will be null by default but still exist in the output Person class instance
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {

            //the Person class is instantiated, and has its properties filled out in this method, according to the custom logic provided
            //in this example, firstName and lastName ARE NOT fields of the Person class, however are expected in the request as Key value pairs, so we validate and extract those two and combine them into the Person.Name property manually
            //when the custom binder is applied, this custom logic will take place over the default Model binding logic that uses attribute rules in the Model class and default correlation rules from requestData to Person model instance
            Person person = new Person();
            string firstName;
            string lastName;
            
            //within a custom model binding, we rely on extracting values from from request context using ValueProvider
            //GetValue from ValueProvider returns object of ValueProviderResult type, which may have more than one result, say "FirstName" is provided as Key several times in the form by some mistake, Count can be > 1.
            //here we check if both FirstName and LastName KVPs are present
            if (bindingContext.ValueProvider.GetValue("FirstName").Length > 0 && bindingContext.ValueProvider.GetValue("LastName").Length > 0)
            {
                //the point is more than one key in request can have the same name, so here we are fetching the first one of however many results, FirstValue and [0] are two ways
                firstName = bindingContext.ValueProvider.GetValue("FirstName").FirstValue;
                lastName = bindingContext.ValueProvider.GetValue("LastName").Values[0];

                //other operations could be done, such as setting correct upper/lower case, or receinving fullName and then splitting into first and last, the point is through custom model binding, the serverside data model that is used can be obfuscated from the expected incoming request data, as the serverside data model may have different property names and even values, custom model binding allows manual "decoding" the requestData into serverside data model/structure
                //here the incoming firstName and lastName, now in serverside memory extracted from the requestData, are trimmed and concatenated into one Name string, assinged to the instantiated Person class
                person.Name = $"{firstName.Trim()} {lastName.Trim()}";
            }

            //additional fields of the Person class would be checked in incoming request through value provider, and assigned to the instantiated Person class
            //the Person class's fields would need to one by one be processed here in the custom model binder, otherwise their values will be null in the returned instantiated object of Person class 
            if (bindingContext.ValueProvider.GetValue("IncomingKey").Length > 0)
            {
                //after checking that the incoming key contains values, retrieve the first instance into memory
                var runtimeVariable = bindingContext.ValueProvider.GetValue("IncomingKey").FirstValue;

                //process/clean validate data
                
                //assign to instantiated target model class's field
                person.DateOfBirth = Convert.ToDateTime(runtimeVariable);
            }


            //the instantiated Person object now must be returned
            //storing the result of the attempted custom model binding
            //passing the instantiated class object, which will become the parameter Person person of the actionMethod for which the model binding is taking place
            //in this example involving the name of a customer, the requestData may have FirstName = "   tom" and LastName = "s and  .ers"
            //through custom logic, one could trim the empty spaces and symbols, properly capitalize and store the name as "Tom Sanders", so custom model binding can also clean/validate incoming request data
            bindingContext.Result = ModelBindingResult.Success(person);
            return Task.CompletedTask;
        }
    }
}
