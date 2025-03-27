using Microsoft.AspNetCore.Mvc;
using ViewComponentsExample.Models;

namespace ViewComponentsExample.ViewComponents
{
    //either this attribute or the ViewComponent inheritance below
    //are needed to acquire some useful functionality, but not required
    [ViewComponent] 
    public class TableViewComponent : ViewComponent
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
            //here an example model is passed to be used to populate the table
            PersonTableModel personTableModel = new PersonTableModel()
            {
                Title = "Person List",
                Persons = new List<Person>()
                {
                    new Person() { Name = "John", JobTitle = "Chief"},
                    new Person() { Name = "Paul", JobTitle = "OffalSeller"},
                    new Person() { Name = "Deseken", JobTitle = "Office"},
                }
            };
            ViewData["Table"] = personTableModel;
            ViewBag.Persons = personTableModel; //alternative
            
            //the target view file name can be different than Default.cshtml, but then must be specified
            //inside the View("customViewName") as the example below
            //will look for Views/Shared/Components/Table/AltTable.cshtml file
            return View("AltTable");

            //while the method returned gives the impression of returning a full view
            //in fact it returns the view component, that is a special partial view
            //default location would be:
            //Views/Shared/Components/Table/Default.cshtml
            //where Table is the first part of this classname -ViewComponent suffix
            //this default expected location of the target component is required or error will occur
            return View();
        }

        //this example below shows the use of parameter passed to the view component, for example if
        //Index.cshtml has a <vc:table2 tableData="ViewBag.ComponentData"></vc:table2> element,
        //note that tableData parameter NEEDS to be same name as parameter received below by InvokeAync()
        //public async Task<IViewComponentResult> InvokeAsync(PersonTableModel tableData)
        //{
        //    return View("AltTable", new { Table = tableData});
        //}

    }
}
