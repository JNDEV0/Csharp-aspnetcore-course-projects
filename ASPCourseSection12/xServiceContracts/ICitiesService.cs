namespace xServiceContracts
{
    //the interface is never manually instantiated, nor is the target Service class
    //CitiesService.cs implements and depends on ICitiesService, which the Controller calls
    //what is defined in the interface, is what can be called, passed parameters to, returned etc
    //between the Service and Controller
    public interface ICitiesService
    {
        Guid ServiceInstanceId { get; }
        List<string> GetCities();
    }
}
