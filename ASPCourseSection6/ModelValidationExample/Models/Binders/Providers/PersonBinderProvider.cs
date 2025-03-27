using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace ModelValidationExample.Models.Binders.Providers
{
    //all this is just to eliminate the need to use the [attribute] mentioned above next to the model class parameter each time
    //the custom BinderProvider must be added to the app builder, see Program.cs
    public class PersonBinderProvider : IModelBinderProvider
    {
        //a custom model binder provider is a class that can be created to define a reusable custom binder associated with a specific model class, in this case Person. so instead of having to name the custom model binder as ex: [ModelBinder(BinderType = typeof(PersonModelBinder))], this attribute can be skipped entirely, and instead the app will identify that if the Person type is passed as a parameter to an actionMethod and model binding should take place, it will first look for the CustomModelBinding when added correctly to the array of binder providers, and if the class is another type than Person, this custom rule will be skipped.
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            //the ModelType here will be the type of the Model being instantiated as the actionMethod parameter, so if:
            //
            if (context.Metadata.ModelType == typeof(Person))
            {
                return new BinderTypeModelBinder(typeof(PersonModelBinder));
            }

            //if target Data model is not Person, return null, will skip this BinderProvider
            return null;
        }
    }
}
