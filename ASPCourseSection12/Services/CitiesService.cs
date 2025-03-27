using xServiceContracts;

namespace xServices
{
    //the naming convention for services are the table/collection name suffixed by Service
    //ie: EmployeesService, AddressService, TaxesService, ShippingCostService etc
    //middleware logic, validations, business logic should be relegated to services
    public class CitiesService : ICitiesService, IDisposable
    {
        private List<string> _cities;
        private Guid _serviceInstanceId;
        public Guid ServiceInstanceId { get { return _serviceInstanceId; } }

        public CitiesService() 
        { 
            //for brevity, an example list is being instantiated
            //this data would otherwise be retrieved from client or database
            _cities = new List<string>()
            {
                "LilTown",
                "BigCity",
                "HalfwayTown",
                "Fartown"
            };

            _serviceInstanceId = Guid.NewGuid();
        }

        public List<string> GetCities()
        {
            return _cities;
        }

        public void Dispose()
        {
            //assuming a database connection to access the cities list,
            //the conneciton would be closed here before the service is disposed of
            
        }
    }
}
