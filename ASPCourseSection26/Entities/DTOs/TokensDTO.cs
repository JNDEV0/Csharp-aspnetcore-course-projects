using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitiesManager.Core.DTOs
{
    //created a TokenDTO object to receive the JWT and refreshToken request from client or external API that handles token generation, creating controller Route with ActionMethod here called GenerateNewToken, since in this example the request to refresh is received from the client, in production this would be entirely handled serverside since client does not need the refreshToken.
    public class TokensDTO
    {
        public string? JwtToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
