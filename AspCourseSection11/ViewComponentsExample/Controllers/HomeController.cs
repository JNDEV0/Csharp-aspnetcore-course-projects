using Microsoft.AspNetCore.Mvc;
using ViewComponentsExample.Models;

namespace ViewComponentsExample.Controllers
{
    public class HomeController : Controller
    {
        [Route("/")]
        public IActionResult Index() //right click name of actionMethod to easily create target view file
        {
            return View(); 
        }

        [Route("about")]
        public IActionResult About()
        {
            return View();
        }

        [Route("about/table")]
        public IActionResult AboutTable()
        {
            //returning a ViewComponent is equivalent o an async partial view where it will only return the component
            //directly, and then frontend code can update the page with the data without reloading the entire view
            PersonTableModel personTableModel = new PersonTableModel()
            {
                Title = "Person List",
                Persons = new List<Person>()
                {
                    new Person() { Name = "View Component", JobTitle = "Chief"},
                    new Person() { Name = "Partial View", JobTitle = "OffalSeller"},
                    new Person() { Name = "Equivalent", JobTitle = "Office"},
                }
            };
            return ViewComponent("Table3", personTableModel);

            //return new ViewComponentResult() { };
        }
    }
}
