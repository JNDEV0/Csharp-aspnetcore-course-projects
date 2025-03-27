using Entities;
using ServiceContracts.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts
{
    public interface IMockDataUtil
    {
        public IPersonsService mockPersonsService { get; set; }
        public ICountriesService mockCountriesService { get; set; }
        public (List<PersonResponse>, List<CountryResponse>)? MockData { get; set; }
        public (List<PersonResponse>, List<CountryResponse>)? FetchMockData(out List<PersonResponse>? personsList, out List<CountryResponse>? countriesList);
    }
}
