//comments with explanations of this content is in previous projects
//see Controller and models class for model binding explanation

//form data is the highest priority in fetching data from requests.
//form data has two types which are enconded as KVPs in the request body
//ContentType application/x-www-form-urlencoded AND multipart/form-data
//form-data can encode files,images etc, and large amounts of KVPs

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.Run();
