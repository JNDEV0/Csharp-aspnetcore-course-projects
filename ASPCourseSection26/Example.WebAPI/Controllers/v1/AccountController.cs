using Asp.Versioning;
using CitiesManager.Core.DTOs;
using CitiesManager.Core.Identity;
using CitiesManager.Core.ServiceContracts;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Common;
using System;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using System.Text;

//this entire controller and its systems are intended for a client application to access the server's routes, see CitiesManager.ClientApp project of the solution
namespace CitiesManager.WebAPI.Controllers.v1
{
    //note that the controller has [allowanonymous] tag to allow requests to reach this route SKIPPING AUTH middleware provided by Identity package, since user will access this controller's action methods as a starting point of registration and login to retrieve a secure token , which the client will then send along with future requests to access routes that require identity authentication/valid session token.

    [AllowAnonymous]
    [ApiVersion("1.0")]
    //note that AccountController inherits from CustomControllerBase the [apicontroller] and [Route] tags so route is domain.com/api/v1/
    //[Route("account")] //setting a [Route] tag here OVERRIDES the inherit so this would be domain.com/account, and [ApiVersion] tag would be irelevant
    public class AccountController : CustomControllerBase
    {
        //note the frequent use of the CustomUser and CustomRole type classes, they are core domain level classes that are used by the various systems provided by the Identity package, to define a user instance's properties, and a userRole's properties, with the aim of diferentiating between user's account types and access levels within the system, for example newUserUnvalidatedEmail has LowUserAccessRole and UserLv3Validated has FullUserAccessRole, and UserAdmin has UnrestrictedAccessRole
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;

        //injecting the JwtService to AccountController, that is tagged [allowanonymous] which bypasses the global filter set at builder.Services.AddControllers, that has enabled authentication for all controllers, if the global filter was not set, authentication WOULD ONLY apply to [Authorize] tagged controllers
        //this CustomService interface is used here to generate a Jwt token and send it to the client once user has authenticated Email and password
        private readonly IJwtService _jwtService;

        //the constructor imports DI services UserManager SigninManager and RoleManager
        //which are used by the actionmethods to handle the login request and add user to database tables through EfCore
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager, IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
        }

        //postregister actionmethod receives POST type request,
        //with the parameterized DataTransportObject properties being validated by the [annotations] of RegisterDTO properties,
        [HttpPost("register")] //postregister actionmethod receives POST type request only
        public async Task<ActionResult<AppUser>> PostRegister(RegisterDTO registerDTO)
        {
            //Validation if ModelState.IsValid == false returning all validation errors in Problem() response
            if (ModelState.IsValid == false)
            {
                string errorMessage = string.Join(" | ", ModelState.Values.SelectMany(whereValue => whereValue.Errors).Select(foundError => foundError.ErrorMessage));
                return Problem(errorMessage);
            }
            //note that the [Remote] tag of RegisterDTO email property also calls the IsEmailUnique method of this controller, to validate the email is not in use already

            //after DTO property validations from incoming request, for brevity here simply creating a User and sending to userManager, the IdentityResult.suceeded provides result from userManager inserting the new registering user into the database.
            //in a real production scenario registering a user to a database is much more intensive, with further validation on each property, customized error messages for each invalid validation property and clientside recovery and user feedback, email/transaction validation 2fa step, encryption and anonymization of data stored in database, should not have any plain string values linked to personal user information, validating the user is unique etc, in this example these extra steps are not implemented as the goal is to showcase the basic functionality.
            AppUser user = new AppUser()
            {
                Name = registerDTO.Name,
                Email = registerDTO.Email,
                UserName = registerDTO.Email, //using the email as Username, could be a string, a public key etc
                PhoneNumber = registerDTO.PhoneNumber
            };

            //after all incoming request validations to ensure the data is in the correct format and valid, that the user is not already registered
            //the generated CustomUser sent to userManager, that will async generate the user in the database with given password,
            //note the user will have columns/properties as stipulated by the CustomUser properties
            //ultimately CustomUser is an object entry in database and password is a string, these can be adapted to use different data types for authentication
            //adding the new user to the users table in Database through Identity UserManager here named result
            try
            {
                IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);

                //debugging, the password is valid after creating async
                //bool isPasswordValid = await _userManager.CheckPasswordAsync(user, registerDTO.Password);
                //if (!isPasswordValid)
                //{
                //    return BadRequest("Invalid password.");
                //}

                if (result.Succeeded)
                {
                    //if user added to databse, returning Ok response
                    //in production might redirect to signed in page, or redirection handled ClientSide
                    var resultLogin = await _signInManager.PasswordSignInAsync(user, registerDTO.Password, isPersistent: false, lockoutOnFailure: false);

                    //after successful login, and if automatic login at registration, calling the _jwtService.CreateJwtToken(user) to retrieve the token, returning with response, note the AuthResponse is a DTO with additional data asside from the token 
                    AuthResponse response = _jwtService.CreateJwt(user);

                    //in this example the CreateJwt() method is including the GeneraterefreshToken() in the authResponse
                    //assigned to the user's RefreskToken property, the point is the refresh token is generated after JWT and then stored to user during the same registration/login logic, UpdateAsync to update the database called on userMAnager(user) since the refreshToken property has been assigned/changed
                    user.RefreshToken = response.RefreshToken;
                    user.RefreshTokenExpiration = response.RefreshTokenExpiration; //also adding RefreshTokenExpiration to user database details


                    //as a summary of the refreshToken, after the JWT token is generated, the JwtService.GenerateRefreshToken method is called, that should return the refreshToken and refreshTokenExpiration, which are then stored to AppUser's properties, and UpdateAsync is called to save the data in database. the client has no use for refreshToken, its used by the server to decide for how long to keep refreshing the JWT while user has an active session, before forcing user to login again to continue, this implementation is very simple here, not monitoring the user's activity, refreshing the refreshToken to persist sessions as long as needed, but this is the basic functionality, the point is the refreshToken has a longer expiration and is used serverSide to recreate a valid JWT for ongoing/frequent requests in an authed session, if a user goes afk and the token expires, the user will have to login again and refreshTokens are deleted on user logout.
                    //in an example implementation, the client has received the JWT and refreshToken, and stored it clientside as localstorage, then the client sends a request to refresh the JWT token, in which both tokens are sent to server, just to illustrate how it works, BUT THIS IS NOT HOW ITS DONE IN PRODUCTION, dont send the refreshToken to client.
                    //in production the server could store the refreshtoken in database / redis, and periodically retrieve it when receiving a request with ONLY valid JWT, to refresh the token for active ongoing sessions, instead of relying on the clientside to send refresh request with valid JWT and refreshToken, which is unreliable but done in this example for brevity
                    //see AccountsController
                    await _userManager.UpdateAsync(user);
                    return Ok(response);

                    //in a production scenario this could have additional steps like confirmation email,
                    //send 2fa validation, perhaps return a response for client to reroute the userClient to a sucess/account login page.
                }
                else
                {
                    //if result.Succeed is false, simply returning the error message of the userManager.CreateAsync operation, in production possibly reattempts or further validation/troubleshooting serverside could be performed to attempt to recover rather than failing the critical user acquisition step of registering
                    return BadRequest(result.Errors);
                }
            }catch (Exception error)
            {
                
                return Problem(error.InnerException.Message);
            }
        }

        //note Logout and Login methods below return Task<IActionResult>, even though various results can be returned like Ok, NoContent, Problem etc, including the nameless object with the user's name and email on login, all these fit as result of IActionResult, or changed as needed or custom ActionResults implemented
        //PostLogin actionMethod will receive POST type requests, receiving LoginDTO as parameter,
        [HttpPost("login")]
        public async Task<IActionResult> PostLogin(LoginDTO loginDTO)
        {
            //ModelState validation and returning a Problem response with validation errors if any
            if (ModelState.IsValid == false)
            {
                string errorMessage = string.Join(" | ", ModelState.Values.SelectMany(whereValue => whereValue.Errors).Select(foundError => foundError.ErrorMessage));
                return Problem(errorMessage);
            }


            //if user is not found by the email, its not a registered user, returning to prevent login 
            AppUser? fetchUser = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (fetchUser is null)
            {
                return NoContent();
            }

            //debugging checking password of fetched user, should confirm its valid here, unless theres an issue with the stored passwordHash
            //bool isPasswordValid = await _userManager.CheckPasswordAsync(fetchUser, loginDTO.Password);
            //if (!isPasswordValid)
            //{
            //    return BadRequest("Invalid password.");
            //}

            //decryption is skipped for brevity here but in production its required if transporting / handling password data at all, currently the password arrives in plaintext from client to server, should would be encrypted manually on clientApp with a serverSecret or through a transport protocol like HTTPS or HTTP/3
            //the manager method is PasswordSignInAsync if the User object is not retrieved already, and SignInAsync if the User is already defined, perhaps the User is retrieved from database in a previous validation step.
            //this method returns SignInResult
            //notes on this section, the params of PasswordSignInAsync are username here email happens to be used as username, a password that is NOT encrypted by default, so a password should only ever be transmitted over an HTTPS or other encryption method client to server
            //the isPersistent: false param makes the token expire / notPersist once browser session ends, lockoutOnFailure requires configuring x attempts before lockout, further confirmation perhaps route to password reset etc
            var result = await _signInManager.PasswordSignInAsync(fetchUser, loginDTO.Password, isPersistent: true, lockoutOnFailure: false);

            //the JsonWebToken is an encrypted string generated post user authentication, that is sent back to the client,
            //client sends the JWT token with each new request as a way of validating subsequent pre-authenticated requests coming from the same session.
            //as a base measure on its own its not very effective in the sense that a third party with username and password can simply login and generate their own JWT token to perform any authed action, but JWT is an extra layer of protection to isolate requests to individual session, the attacker would need to have access to the user's computer files WHILE a valid session is ongoing to steal the valid token in order to hijack that authed session.
            //additional checks can/should be paired along with token, like user/pass auth, ip/region tracking of session requests, session length expiration, 2fa token validation etc
            //so the JWT token is intended as a lay of protection for the authed session's requests, like modifying a user's profile.
            //the basic JWT is made up of a few components:
            //the header that includes token type and hashing algorithm,
            //the payload that are KVPs of post-validation data sent back to the client as part of the token, the payload not used in any way clientside as its ultimately encoded, point is a user specific payload of data
            //last part is the "secret" kept by the server, even if its a GUID generated for a user session or regenerated at server startup, the secret part is known only to the server and stored in runtime or env variables.
            //the header and payload are then base64encoded, which is simply converting the human readable string into an encoded format(not encrypted, can be reversed), and the encoded parts are then hashed using the algorithm mentioned in the header, and the hashed output is considered the "JWT signature" which is then appended to the encoded "data" and that is the token. 
            //the client finally receive the token in response and stores in clientSide variable to attach to future requests,
            //the point is if the token given by incoming request has invalid header, payload, or secret, that is, ANY of the data the server expects, perhaps the userId is wrong, or an attacker makes up a different secret, the VALID token will be different from incoming req INVALID madeup token, and since the server will recreate what it expects the valid token to be on each new request, if the incoming token is different than what is expected, its invalid, so the server deciding that 10minutes has passed and the token is invalid theres one of the details that changed in the EXPECTED token output, so the previously valid token now reads as invalid for having the wrong timestamp as component in its hash string output

            //he result of PasswordSigninAsync call is then checked using .Succeeded property,
            //in this simple example if signin works the fetched AppUser by username/email is used to return soem values
            if (result.Succeeded)
            {
                //if signin succeeded returning object with some relevant user details and token to prove login succeeded
                //sensitive information like password or entire profile would not be returned here 
                AuthResponse response = _jwtService.CreateJwt(fetchUser);
                
                fetchUser.RefreshToken = response.RefreshToken;
                fetchUser.RefreshTokenExpiration = response.RefreshTokenExpiration; //also adding RefreshTokenExpiration to user database details
                await _userManager.UpdateAsync(fetchUser);

                return Ok(response);
            }
            else
            {
                //at this point a valid email is being used BUT wrong password, simple example returning Problem() response, but in production this could involve handling lockout, automatic password reset, block IP if spamming attempts etc.
                //the else case returning Problem simplifying the response in case the signin fails, that there is an issue with email or password in client response
                return Problem();
            }
        }

        //ClaimTypes are added when the JWT token is generated, into the Claims[] payload,
        //in this case a ClaimTypes.Email has been added with the user's email, and retrieving the ClaimTypes.Email from the ClaimPrincipal extracted securityToken works, if the ClaimType was another, the implementation would have to be adapted
        //the point of claims is that its supposed to be a uniqueValue to the user, and its not a completely random value each time in the payload, since the JWT will be extracted later to refresh the token, the data contained needs to be consistently verifiable, thats why the claimTypes is not a random GUID for example, but it could be if it the value was stored with the user's profile in database and was retrieved as validation of the token.
        //in this example the clientside was given a "refreshToken" UI button and is performed manually, but this is not the real world solution since users shouldnt have to manually refreshing their login. in production a middleware serverside could inspect the incoming token to see if it is expired and refresh it automatically to persist a user's authed active session as long as needed, without user input, thats why the JWT token is given a short expiration with the refreshtoken given a longer expiration, stored as variables at appsettings.json in this example
        [HttpPost("refresh-jwt-token")]
        public async Task<IActionResult> RefreshJwtToken(TokensDTO tokensDTO)
        {
            //in production actual modelbinding validation would be performed to ensure the tokens are at least in valid format
            //also this method can be internal, instead of given a HttpPost route, could be called by a middleware that refreshes tokens automatically
            if (tokensDTO == null) return BadRequest("placeholder validation");

            //to refresh/create a new jwt, first ClaimsPrincipal is extracted from the payload of the valid/expired-yet-valid token
            try 
            { 
                //the ClaimsPrincipal is now returned to the controller, where the initial payload component/claims that were stipulated when the original JWT token was issued can be validated,
                //the steps up to this point essentially unpacked the payload of the incoming JWT token,
                ClaimsPrincipal principal = _jwtService.GetPrincipalFromJwtToken(tokensDTO.JwtToken);
                
                //searching the principal for the value of ClaimTypes.Email that was set to receive the Email address when the token was created in the Claims[] payload, see CreateJwt(), keeping in mind at this stage we are expecting a token that has ALREADY been created, is unpacked, and only because Email was included as a payload component that we can expect to find it now
                string? email = principal.FindFirstValue(ClaimTypes.Email);
                
                //and now the user is retrieved from database using the email that was extracted from the claims/ payload,
                AppUser? user = await _userManager.FindByEmailAsync(email);

                //if the user is not found or
                //if user.refreshToken is not the same as the requestDTO refreshToken
                //if the DateTime stored as Expiration is less/inThePast compared to now(meaning the token is already expired valid),
                if (User == null || user.RefreshToken != tokensDTO.RefreshToken || user.RefreshTokenExpiration <= DateTime.Now)
                {
                    //returning a BadRequest WHICH DOES NOT include a new JWT token.
                    //in production, this would not return a HTTP response to client, since the server should handle refreshing tokens without clientside requests.
                    //instead this means the token or database data is somehow corrupted,
                    //since the email is included by default in the issued token,
                    //the refreshToken of the user is added on login or registration,
                    //or the incoming token is simply not expired yet 
                    return BadRequest("Invalid refreshToken");
                }

                //after the validations, CreateJwtToken(user) is called with the user retrieved using the email retrieved from the valid JWT's payload that had the ClaimPrincipal extracted, CreateJwtToken will generate a new token for the user and new refreshToken,
                //the AuthResponseDTO returned by it is used to set the user's refreshToken and expiration manually here,
                AuthResponse authResponse = _jwtService.CreateJwt(user);
                
                //then userManager.UpdateAsync is called to update the user in database after changes,
                user.RefreshToken = authResponse.RefreshToken;
                user.RefreshTokenExpiration = authResponse.RefreshTokenExpiration;
                await _userManager.UpdateAsync(user);

                //and authResponseDTO is returned to client, containing the new JWT and refreshToken to client, that presumably stores them in localstorage or clientSideMemory through the clientApp's implementation for future authed requests or further refreshToken requests
                //in production this would not return an HTTP response directly to the client, perhaps would store the new refreshToken, and then package the new JwtToken into the client response for future authed requests.
                return Ok(authResponse);
            }
            catch(Exception ex) 
            {
                //in this example, if the token given to GetPrincipalFromJwttoken is invalid, an error is thrown.
                //this entire route refresh-jwt-token is an example where the client is expected to send in the refreshToken, but in production this method would be internal and not return an HTTP response like this, since refreshingToken is handled by backend server/api
                return BadRequest("invalid token");
            }


        }

        //the GetLogout actionmethod calls SignOutAsync and returns no content here,
        //in production perhaps redirect user to home page, invalidate session keys, etc.
        [HttpGet("logout")]
        public async Task<IActionResult> GetLogout()
        {
            await _signInManager.SignOutAsync();
            return NoContent();
        }

        //IsEmailUnique(email) is called by the [Remote] annotation tag on the Email RegisterDTO property,
        //by default its a GET request sent by [Remote()] and passes the annotated property value to the associated [remote(actionMethod)],
        //this method returns true/false wrapped in a Ok() response object, to customize the validation of the property of the RegisterDTO, so this method is actually called before the actionmethod logic of RegisterPost() actionmethod above,
        [HttpGet]
        public async Task<IActionResult> IsEmailUnique(string email)
        {
            //here async checking of UserManager finds a ApplicationUser by the provided Email, note that FindByEmailAsync will propagate an error from the UserStore if options.User.RequireUniqueEmail is not enforced and there are duplicates in the database,
            //so this actionmethod is validation functionality wrapped in an actionMethod, called by the model binding step of handling the user request for simplicity its added on the same controller here, could be a separate controller dedicated to DTO validations
            AppUser? userFound = await _userManager.FindByEmailAsync(email);
            if (userFound is null) return Ok(true);
            else return Ok(false);
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestMethod()
        {
            return Ok();
        }
    }
}
