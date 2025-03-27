using Microsoft.AspNetCore.Mvc;
using ViewComponentsExample.Models;

namespace ViewComponentsExample.ViewComponents
{
    //either this attribute or the ViewComponent inheritance below
    //are needed to acquire some useful functionality, but not required
    [ViewComponent] 
    public class Table2ViewComponent : ViewComponent
    {
        //this is a required method for view components, sice it is invoked by the server
        //when the view component is called, this TableViewComponent class will be instantiated
        //and InvokeAsync() will be called automatically
        public async Task<IViewComponentResult> InvokeAsync()
        {
            //other code logic can go here, such as fetching data from database,
            //ViewBag data from the View that is calling this component,
            //etc to populate the view component.

            //data model and viewData can be used to pass data to the view component being returned by View()
            //here an example model is passed to the strongly typed view be used to populate the table
            PersonTableModel personTableModel = new PersonTableModel()
            {
                Title = "Person List",
                Persons = new List<Person>()
                {
                    new Person() { Name = "JohnModel", JobTitle = "Chief"},
                    new Person() { Name = "PaulModel", JobTitle = "OffalSeller"},
                    new Person() { Name = "DesekenModel", JobTitle = "Office"},
                }
            };

            //the target view file name can be different than Default.cshtml, but then must be specified
            //inside the View("customViewName") as the example below
            //will look for Views/Shared/Components/Table2/Default.cshtml file
            //note that inside the target view cshtml file the @Model property will have the above data passed to it
            return View(personTableModel);
        }
    }
}
