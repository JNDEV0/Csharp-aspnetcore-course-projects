namespace EnvironmentsExample.AppOptions
{
    //the naming convention is not enforced here, nor the AppOtions folder.
    //the point of an Options class is that it can be named as a model class
    //for environment variables retrieved from appsettings.json through the IConfiguration Dependency Injection

    public class WeatherApiOptions
    {
        //names of fields should be the same as the target keys
        //when Get<OptionsModel>() is called it will populate the fields
        //in this example see HomeController.cs
        public string? ClientId { get; set; }
        public string? ClientSecretKey { get; set; }
    }
}
