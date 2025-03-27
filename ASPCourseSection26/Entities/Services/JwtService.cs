using CitiesManager.Core.DTOs;
using CitiesManager.Core.Identity;
using CitiesManager.Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CitiesManager.Core.Services
{
    //summary of jwtService customService class that handles the token generating algorithm, up to this point DTO and customService are created, now the service needs to be injected/called from the controller to actually return the token with login response
    public class JwtService : IJwtService
    {
        //configuration imported because this example stores Jwt data like issuer, audience, expiration and secret in appsettings.json, that configuration parses data from. in production these datapoints may be dynamic according to host/client, secret would be stored securely in OS env variables
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //the response can contain other plaintext data, AuthResponse is essentially a customDTO that also transports the token hashed property, now in string format when sent with response, even though its readable its an encoded and encrypted string that reveals no data to the client, server generates and keeps track of the JWT token usage, the client does not need to check it or do anything to it aside from sending it with future requests, while the token is valid.
        public AuthResponse CreateJwt(AppUser user)
        {
            //storing a DateTime obj set to now+EXPIRATION_MINUTES stored in appsettings.json
            DateTime expiration = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:EXPIRATION_MINUTES"]));

            //the JWT "payload" is composed in Claim objects, these details are encoded then hashed, so these details are not unpacked by the client in any way,
            //the point is the payload along with the secret securityKey define the output, so if any of these details are something OTHER than expected by server, token is invalid
            Claim[] claims = new Claim[]
            {
                new Claim("Id", Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), //unique Id associated with the subscribing user, can be something other than userId but needs to be user unique
                new Claim(JwtRegisteredClaimNames.Email, user.Email), //identifierProperty by which user can also be identified, aside from the sub uniqueId, this is optional
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), //JwTokenId, when generating a new token, it needs a uniqueId of its own, here generating a new Guid
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()), //issuedAtDateTime

                //new Claim(ClaimTypes.Name, user.Name) //identifierProperty, optional
            };

            //the security key is a serverside secret string that is used in the encryption process, this key is NEVER sent to client,
            //here its simply stored in configuration appsettings.json, in production this would be secured in env variables, password protected encrypted vault etc
            //its the essential part to the utility of the JWToken. if an attacker somehow acquires the secret/securityKey, they could extrapolate/reverseEngineer the serverside logic based on un-encoding the payload and remotely calculate JWT tokens the server would consider valid, this would be a vector of attack on the server. explanation to point out the importance of secreting away the securityKey only for server access
            //GetBytes is called to convert the securityKey into a byte array required in next step
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));

            //the credentials are a config for next steps, where the encoded(not yet encrypted) securityKey is paired with a target encryption algorithm
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //named tokenFactory because the token itself is not generated yet,
            //there are several overloads of JwtSecurityToken, with different param sets
            //in this example the issuer is this app's domain:port and audience is allowed client domain:port, to only allow valid tokens if they are between these two addresses a setting for securing an API to serve only a specific "client", that may itself be another webServer
            //JwtSecurityToken tokenFactory = new JwtSecurityToken(
            //    _configuration["Jwt:Issuer"],
            //    _configuration["Jwt:Audience"],
            //    claims,
            //    expires: expiration,
            //    signingCredentials: credentials);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
            {
                    //the Claims[] payload components although they will ne encoded and encrypted into the JWT token, the details are later important, as various validations are performed on extracted Claim data, such as validating an expired token to refresh it
                new Claim("Id", Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()), //issuedAtDateTime
            }),
                Expires = expiration,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = credentials
            };

            //the JwtSecurityTokenHandler class is instantiated and its WriteToken method is passed the JwtSecurityToken
            //the output is the encoded header and payload hashed with the chosen SecurityAlgorithms.HmacSha256,
            //then the tokenFactory/signature is base encoded, and the output result token string is a concatenation of hashedEncryptedDataPayload + signature
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            var tokenCreate = tokenHandler.CreateToken(tokenDescriptor);
            string token = tokenHandler.WriteToken(tokenCreate); //at this point the JWT token is effecively ready
            
            //older version could pass a tokenGenerator/factory type object directly to write Token,
            //but NOT calling CreateToken deprives some info critical to the process apparently, so using the TokenDescriptor method instead
            //string token = tokenHandler.WriteToken(tokenFactory);

            //customDTO AuthResponse to transport the response from this service class to the controller that will call this DI service
            //This method could return just the token string, but since it uses the AppUser data anyways to generate the Jwt,
            //other client useful data is returned in this response, since the token cannot be easily reversed to extrapolate details like tokenExpiration for clientside processing
            return new AuthResponse() { Email = user.Email, Name = user.Name, Expiration = expiration, Token = token, RefreshToken = GenerateRefreshToken(), RefreshTokenExpiration = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["RefreshToken:EXPIRATION_MINUTES"]))}; //token expiration (10min) retrieved from appsettings.json and added to current time
        }

        //adding GenerateRefreshToken method to JwtService.cs, note the RNG generator recomended .Create() method instead of new(), the point is a cryptographically strong number, other RandomNumberGenerator methods can be used
        //the base64string of the byte array populated by randomNumberGenerator.GetBytes() here is used as a refreshToken, note that the refreshToken is much simpler than the JWToken, because ultimately it does not need to be exposed to the client
        //in this simple example the GenerateRefreshToken() is called above when returning authresponse, but in production this should not happend
        //for ilustration in this example the refreshtoken is being sent and received to/from client, it should NOT be returned to client in production env, instead refreshToken is stored with appUser profile in database provider and when user logout or token expires, the system should log the user out and prompt sign in again, the refreshToken is used to refresh and keep a validated authed session going, as long as active, thats why the JWtToken has a shorter expiration.
        private string GenerateRefreshToken()
        {
            Byte[] bytes = new byte[64];
            var randomNumberGenerator = RandomNumberGenerator.Create();

            randomNumberGenerator.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public ClaimsPrincipal GetPrincipalFromJwtToken(string token)
        {
            //implementing in JwtService the GetPrincipalFromJwtToken method that will receive a string of the valid JWT token sent to the controller route, that actionmethod will call this GetPrincipalFromJwtToken method in JwtService through IJwtService DI service to the controller constructor.
            //note also that the TokenValidationParameters does NOT validate the Lifetime, at this point, since the token may already be expired that we are refreshing, what matters is that its a valid JWT token so Validate settings check the audience, issuer, payload encryption signature/ secretKey, etc
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"])),
                ValidateLifetime = false, //note that since this method is used to extract data from a valid jwt token, wether or not its expired
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            //notice the ClaimsPrincipal principal variable, it is calling the ValidateToken(token, params, out resultSecurityToken), that will verify the token according to the given params, and output result, if result is NOT of JwtSecurityToken type it is null and therefore the validation failed, or also if the !secToken.header.alg.equals(encryptionAlgorithmUsed) the algo should be in the valid  JWtToken by default if the validation passed.
            //SecurityToken? validateTokenResult;
            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validateTokenResult);

            //patter matching syntax used to typecast the result of validation into JwtSecurityToken,
            //since that is the type expected, the result of ValidateToken() only returns a SecurityToken
            //also checking that the header of the token has the expected encryption algorithm
            if (validateTokenResult is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)) 
            {
                //this validation should never fall in here, unless someone is tampering with the token or the data is corrupted.
                //instead of throwing an error, could perhaps log the user out or perform further security validations/logging
                throw new SecurityTokenException("Invalid JWT token, unable to execute JWtServices.GetPrincipalFromJwtToken()");
            }

            //if above validations check out, returning the extracted claimsPrincipal, the data inside is not validated yet, only extracted from the valid or expired JWT token
            return principal;
        }
    }
}
