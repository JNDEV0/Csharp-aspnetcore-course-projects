//this package is where [FromQuery] and [FromRoute] are from
using Microsoft.AspNetCore.Mvc;

namespace ModelBindingExample.Models
{
    //the data model binding class should contain as fields the parameters expected to be retrieved from the request. order of precedence is default
    //the BookSearch class parameter in the actionMethod will instantiate a BookSearch with its fields and methods, populated with values from the request, which can then be acessed in the actionMethod
    //model binding class can work with incoming requests or outgoing responses
    public class BookSearch
    {
        //[FromQuery] and [FromRoute] can be set as tags to individual model binding fields as well
        public int? BookId {  get; set; }
        public string? AuthorName { get; set; }

        public override string ToString()
        {
            return $"BookId:{BookId} AuthorName: {AuthorName}";
        }
    }
}
