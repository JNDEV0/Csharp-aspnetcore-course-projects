var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.Run();

/*
 * the point of a view component is to organize the content into reuseable components
 * see /ViewComponents/TableViewComponent.cs
 */