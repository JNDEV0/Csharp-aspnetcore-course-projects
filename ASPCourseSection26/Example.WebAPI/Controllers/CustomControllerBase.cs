﻿/*
 this controller class is generated by vscode, the model City class was given with almost empty DbContext class, only stipulating a seed entry OnModelCreating
 a migration was added and updated to the database, that generated the timestamp_Initial_City code executed on the SQL database to create the tables and rows.

 the api methods below were generated for CRUD operations around the city class
 */
using Microsoft.AspNetCore.Mvc;

namespace CitiesManager.WebAPI.Controllers
{
    //no versioning default route is api/CustomcontrollerName-ControllerNameSuffix
    //[Route("api/[controller]")]

    //versioned route, the float valuegiven with api/v0.0/cities for example
    //note that the Controllers like v1/CitiesController and v2/CitiesController have the [ApiVersion] tags
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class CustomControllerBase : ControllerBase
    {
        //[Route("api/[controller]")]
        //[ApiController]
        //ApiController tag enables the route to read from request body in json format automatically
        //without this tag, the route will attempt to read from route appended query string,
        //or use [FromBody] tag given to parameter of City type at actionmethod
        //note also that the Route and ApiController tags as well as inheriting from ControllerBase is required for API
        //a parent CustomControllerBase class can be created, that inherits ControllerBase and applies the [tag] annotations
        //this way grandchild classes, like CitiesController and ExampleController on this example project, can inherit CustomControllerBase
        //and no longer need to repeat the [tags] for routing, and given the api/[controller] route being dynamic, uri wll still be api/example and api/cities
    }
}