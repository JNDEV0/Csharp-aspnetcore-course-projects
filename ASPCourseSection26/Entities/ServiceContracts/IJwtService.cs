using CitiesManager.Core.DTOs;
using CitiesManager.Core.Identity;
using System.Security.Claims;

namespace CitiesManager.Core.ServiceContracts
{
    //note the IJwtService interface used to DI the customService that implements this interface to the controller here has one method,

    public interface IJwtService
    {
        //CreateJwtToken receives the retrieved data used to generate the token either from runtime or database and returns an authenticationResponse,
        //which is a customDTOResponse that expects the valid token, expiration, user details here the name and email,
        //expects a AppUser object, could be a customDTO instead
        public AuthResponse CreateJwt(AppUser user);

        //used by the refreshJwtToken actionmethod of AccountController to extract the payload data from a valid jwt token
        //to verify and refresh/reissue a new jwt token
        public ClaimsPrincipal GetPrincipalFromJwtToken(string token);
    }
}
