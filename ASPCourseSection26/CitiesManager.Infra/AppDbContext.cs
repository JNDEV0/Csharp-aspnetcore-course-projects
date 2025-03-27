using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitiesManager.Core.Entities;
using CitiesManager.Core.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CitiesManager.Infra
{
    //sidenote: the identity package is handled by the program.cs builder.Services settings, and then added in the middleware stack using app.UseAuthentication
    //manages user password profile roleAccess, tokens email confirmation externalAuthLoginservices etc, flow => asp.netcore app(controllers), identityManager, identityStore, identityDbContext, Database
    //at this point the CustomDbContext, here AppDbContext, inherits from IdentityDbContext<T1,T2,T3> where T1 is CustomUser, T2 CustomRole, T3 IdType
    //this enables EFCore through migration to generate the tables, rows etc automatically, including base identity package tables used for authentication,
    //while the CustomDbContext only contains the DbSets collections needed there for the DbContext to function, 
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }

        //note that the only property added here is the collection that will be generated and managed by efcore, and the seed entry in OnModelCreation
        //the identity tables, rows etc are created and managed by Identity package directly
        public virtual DbSet<City> Cities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //initializing one seed data entry to generate DB table/rows
            //note that using Guid.NewGuid() fails because this would generate a different Guid each time,
            //which is not the intention here of detecting a new entry each time the OnModelCreating is ran,
            //so NewGuid would actually fail the migration because of this, instead we parse a static string below
            modelBuilder.Entity<City>().HasData(new City()
            {
                Id = Guid.Parse("0DD9E91D-5A7C-4CC4-8238-1B17A48CE8A2"),
                Name = "Exampleville"
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
