﻿//DTO stands for Data Transfer Object, a model wrapper class used to transport the target data
using Entities;

namespace ServiceContracts.DTO
{
    public class CountryAddRequest
    {
        public Guid? CountryId { get; set; }

        //since this class is intended to wrap the incoming request data from client
        //we expect only the CountryName,
        //later the Country data is parsed out of this class,
        //we add CountryID generated by the server for the resulting Country.cs class instance
        public string? Name { get; set; }

        public CountryAddRequest(string countryName) 
        {
            this.Name = countryName;
        }

        public CountryAddRequest() { }

        //to facilitate retrieving the Country object from CountryAddRequest
        //note that Country.cs is in Entities project in this solution, reference has been added
        //the Country class generates a Guid via constructor, by time this method is called
        //its assumed the input is safe and validated elsewhere
        public Country ToCountry()
        {
            return new Country(this.Name is not null ? this.Name : "InvalidName");
        }
    }
}
