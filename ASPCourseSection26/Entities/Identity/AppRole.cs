using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace CitiesManager.Core.Identity
{
    public class AppRole : IdentityRole<Guid>
    {
        //holds properties related to a given user role/rank, may be properties about what this user type can or cant do por example
        //public bool CanAddCities {get;set;} = true;
    }
}
