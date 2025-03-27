//static files are by default to be stored at wwwroot folder in project solution explorer to the left, which gives the icon by default
//any file in this folder is a public "static file", that file becomes accessible via client browser, by calling that file's name
//the idea is to make media available unconditionally, which then can be packaged into components using code, to display the media in the correct way
//ie: an image resized, centered, part of a click interactive slideshow, instead of displaying the raw full sized image with no context

//static files are a solution to the problem that initially all files, code files etc, were all accesible by the client. 
//this was a security issue, so now only content expressedly marked as static by being in the wwwroot or other custom folder, will be served publicly
//allowing server side logic code files to remain hidden
//important to note that browsers will also create local cache of the received static files, so if a file is later made unavailable, some clients may still have it stored locally

//the WebApplicationOptions has options that can be configured before the builder, but some settings like WebRootPath to designate a custom static files folder
//can only be called by the builder method, so must be configed where it is initialized, in the args
//older versions of C#/.NET would require that wwwroot folder be present even if unused and custom rerouted
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions() { WebRootPath = "customStaticFiles1" });
var app = builder.Build();

//thise default parameterless call to UseStaticFiles uses either the default wwwroot or redefined static folder provided above, customStaticFiles1
app.UseStaticFiles();

//the builder option of setting the WebRootPath will only accept one folder, if there are multiple folders to make available then
//this option below can add multiple static folder routes, note that concatenating builder.Environment.ContentRootPath + "customStaticFiles2"
//is the same as using Path.Combine, essentially pointing the UseStaticFiles call to the additional folder designated to hold static files, since
//ContentRootPath is the directory where the project is located, so by concatenating customStaticFiles2
//inside the PhysicalFileProvider constructor, it will point to that path when looking for public static files, in addition to customStaticFiles1 in this case
app.UseStaticFiles( new StaticFileOptions()
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(builder.Environment.ContentRootPath, "customStaticFiles2"))
    });

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.Map("/", async context =>
    {
        await context.Response.WriteAsync("Hello World!");
    });
});

app.Run();
