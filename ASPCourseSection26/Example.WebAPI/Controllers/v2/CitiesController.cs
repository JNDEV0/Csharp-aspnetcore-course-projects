﻿/*
 this controller class is generated by vscode, the model City class was given with almost empty DbContext class, only stipulating a seed entry OnModelCreating
 a migration was added and updated to the database, that generated the timestamp_Initial_City code executed on the SQL database to create the tables and rows.

 the api methods below were generated for CRUD operations around the city class

    note this is Version 2 of the CitiesController, API versioning may enable or disable functionality according to implementation.
    in this simple example, v2 only allows GetCities, ie: v1 might be enabled for authed users only.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis;
using CitiesManager.Core.Entities;
using CitiesManager.WebAPI.Controllers;
using System.Net;
using System.Xml.Linq;
using Asp.Versioning;
using Microsoft.AspNetCore.Cors;
using CitiesManager.Infra;

namespace Example.WebAPI.Controllers.v2
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

    [ApiVersion("2.0")] //controller tag stipulates the "version" of this controller, see AddApiVersioning() @ Program.cs
    //cors policies are stipulated at Program.cs builder and inserted into the app request middleware pipeline.
    //beyond the DefaultPolicy that applies to all incoming requests, any additional policies added via AddPolicy("customPolicyName", lambda=>{...}) are
    //stored in runtime memory by the app server but ONLY used for controllers or actionMethods that have the [EnableCors("customPolicyName")] stipulated
    //note that even with a global default set, stipulating a custom policy by name here overrides and applies instead of the default, non-cumulatively
    [EnableCors("customPolicyName")]
    public class CitiesController : CustomControllerBase
    {
        private readonly AppDbContext _context;

        //in production, Dependency Injected Service classes with Inversion of Control interfaces
        //would be implemented instead of accessing DbContext here, with Data Transfer Object classes to isolate the Domain Layer data City.cs
        //the operations would be executed from here on the CustomService class that would in turn access CustomDbContext
        //by default from generated code and for example brevity the controller is accessing the DbContext directly for CRUD operations here
        public CitiesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Cities
        [HttpGet]
        //[Produces("application/xml")] //produces and consume tags can override default body type for a specific actionmethod, see Program.cs AddControllers()
        //[Consumes("application/xml")] //in this example the default is application/json, with these two tags GetCities would use application/xml in req. and resp.
        //ActionResult is the base class that implements IActionResult, and many other result classes are derived from ActionResult, like ContentResult, RedirectResult, OkResult, EmptyResult, StatusCodeResult,JsonResult, ViewComponentResult,ObjectResult, NotFoundResult, BadRequestResult, CreatedResult, UnauthorizedResult and NoContentResult
        //a few notes on the ActionResult, for example return type ActionResult with return statement Ok(object) is same as return type ActionResult<object> with return statement object, this is explained to show that these resultTypeMethods and the ActionResult classes work together
        //returning a ActionResult derived result class is best if the response should only be informative, returning IActionResult<T> is best for returning object responses, ObjectResult also derives from ActionResult>IActionResult
        //[EnableCors("customPolicyName")] //just to illustrate that a custom CORS policy can apply to single actionMethod of an endpoint
        public async Task<ActionResult<IEnumerable<City>>> GetCities()
        {
            return await _context.Cities.ToListAsync();
        }
    }
}