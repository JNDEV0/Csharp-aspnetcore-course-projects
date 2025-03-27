using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitiesManager.Core.DTOs
{
    public class AuthResponse
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Token { get; set; }
        public DateTime Expiration { get; set; }

        /*adding a new KVP for RefreshToken expiration, once JWT would expire, if refreshToken is NOT expires, program will issue a new JWT, until refreshToken is NOT valid. perhaps renewing refresh token itself before it expires if long active session ongoing to prevent logging out the active user.
        adding RefreshToken to the AuthResponse server->client to illustrate, but the refreshtoken has no reason to be sent to client, instead refreshtoken needs to be stored serverside/database because the client will send in the short-time span JWT with requests, and having expired the server will retrieve the refreshtoken from database, compare its expiration to the JWT and reissue new JWT if refreshtoken is still valid, 
        this example has the client storing the refreshToken and sending it in with the JWT, but this implementation is not realistic in that people dont click a refresh button clientside to refresh the token, this ican be done with a middleware that validates and refreshes the token
        and refreshtoken cannot be trusted from client requests itd have to be validated server side anyways, so AppUser needs to store it, the client having or sending in refreshtoken is of no use. 
        the refreshToken's utility can be thought of as RefreshActiveSession() without doing all the steps needed on its own, for example:
        an expired jwt token alone does not call _userManager.Signout on the session, but that naturally needs to occur once a token is invalid and not automatically being refreshed. so refreshToken is a step to intercept and improve the active user's experience by prolonging a session -condicionally- to determine how long a JWToken can be renewed per single valid login session via refreshToken, and refreshToken itself can be refreshed to not interrupt hours long sessions, while shortening the window of opportunity for a man-in-the-middle attacker
        */
        public DateTime RefreshTokenExpiration { get; set; }
        public string? RefreshToken { get; set; }
    }
}
