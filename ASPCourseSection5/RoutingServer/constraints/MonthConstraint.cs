

using System.Text.RegularExpressions;

namespace RoutingServer.constraints
{
    //custom route mappings are specified in the endpoints list, custom constraints must be added to the app builder before building
    //custom constraint classes are ways to encapsulate route constraint validation logic
    public class MonthConstraint : IRouteConstraint
    {
        //context is the request/response context of the request, same as all other middleware
        //the route represents the route in which constraint is applied, ie: sessionKey/{hex:guid}
        //custom constraints are setup to handle a 'routeKey' or specific variable, in this case month of sales-report/{year:int:min(2022)}/{month:regex(^(jan|feb|mar)$)}
        //values is a dictionary of possible values for routeKey, like specifying only oct nov dec as valid
        //routeDirection is either IncomingRequest or UrlGeneration, wether this is handling a incoming request or generating a url typically for model view controller
        public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            //if the routeKey route constraint variable value passed such as /sales-report/1996/jan here jan if it is not part of the approved values dictionary
            //return false, which will cause the middleware using this custom constraint to fail the variable check. this makes month variable mandatory
            if (!values.ContainsKey(routeKey))
                return false;

            //if there is some value provided in the variable being checked, attempt to store it into string memory
            //check regex expression for simplicity, any verification can be implemented.
            //note that the actual value passed in the URL of the request, is stored within values[routeKey], if any was provided
            //if verifcation passes, returns true
            string? month = Convert.ToString(values[routeKey]);
            Regex regex = new Regex("^(jan|feb|mar)$");
            if (regex.IsMatch(month))
                return true;
            else
                return false;
        }
    }
}
