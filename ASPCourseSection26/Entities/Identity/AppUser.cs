using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace CitiesManager.Core.Identity
{
    public class AppUser : IdentityUser<Guid>
    {
        //holds properties relating to a given usertype, the app may have multiple usertypes and rolelevels,
        //note that the identity package handles the guid as Id of entity through the IdentityUser<T> inheritance instead of a local Guid Id property in AppUser
        //this user file is intended to hold details about the user that will use EFCore for CRUD database operations, tipically during authentication tasks
        public string Name { get; set; }

        public string PhoneNumber { get; set; }
        //note the IdentityUser already has a Email property, readding here would cause duplicates and the IdentityUser package will ignore this customProperty
        //public string Email { get; set; }

        //the refreshToken is stored with the user profile in database provider, so when a request is incoming with a JWT token that needs to be renewed, the refreshToken can be retrieved from the userProfile and used to generate a new JWT token to maintain an ongoing authedSession, see JwtService.cs
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiration { get; set; }
    }
}
