using Microsoft.AspNetCore.Mvc;
using ViewComponentsExample.Models;

namespace ViewComponentsExample.ViewComponents
{
    [ViewComponent] 
    public class Table3ViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(PersonTableModel personTableModel)
        {
            return View(personTableModel);
        }
    }
}
