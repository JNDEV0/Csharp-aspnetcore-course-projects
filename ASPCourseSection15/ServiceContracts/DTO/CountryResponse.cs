using Entities;
using ServiceContracts.Enums;

namespace ServiceContracts.DTO
{
    /// <summary>
    /// DTO class is used as return type for CoutriesService methods, typically after an incoming request
    /// </summary>
    public class CountryResponse
    {
        public Guid CountryId { get; set; }
        public string? Name { get; set; }

        //public CountryResponse() { CountryId = Guid.NewGuid(); }

        public override bool Equals(object? obj)
        {
            //the default Equals implementation compares two objects which even if they have 
            //the exact same exact data its two references and so not the same
            //return this == obj;

            //so here we add validation to ensure that the generic object type is valid
            if (obj is null) return false;
            if (obj.GetType() != typeof(CountryResponse)) return false;

            //then cast it and compare the field values directly instead
            CountryResponse other = (CountryResponse)obj;
            return this.CountryId == other.CountryId && this.Name == other.Name;
        }

        public CountryAddRequest ToCountryAddRequest()
        {
            return new CountryAddRequest()
            {
                CountryId = this.CountryId,
                Name = this.Name
            };
        }
    }

    
    /// <summary>
    /// Utility class used to convert Country class into CountryResponse using static extension method
    /// </summary>
    public static class CountryUtils
    {
        public static CountryResponse ToCountryResponse(this Country country)
        {
            return new CountryResponse() { CountryId = country.CountryId, Name = country.Name };
        }
    }

}
