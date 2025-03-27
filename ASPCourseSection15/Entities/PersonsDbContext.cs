using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.Intrinsics.X86;
using Azure.Core;
using Azure;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

/*
 * a stored procedure is a way of storing a database operation directly in the db provider.
 * using a stored procedure has three steps:
 * the stored procedure is stored in the db provider through adding the custom operation into a migration
 * then a method is implemented into the CustomDbContext that EXECUTEs the stored procedure,
 * and the method above is called from the CustomService class using the injected CustomDbContext in its constructor
 * the point is the stored procedure may be a complex multistep operation that is best stored and called directly in the database provider 
 * instead of sent along with each request from server
   
    //a "migration" is when a change is made in the database data model/context, where pre existing objects in the database are in an obsolete format that needs to be updated to a new structure,  when there are changes in the data strcuture of a table or collection, to update all the files, the old formatted files are parsed in from the old table/collection, and updated to new format, 

    //open Tools > Package Manager Console set default project to:
    Install-Package Microsoft.EntityFrameworkCore.Design <--target solution startup project, bold name in solution explorer tab of VS IDE
    Install-Package Microsoft.EntityFrameworkCore.Tools <--target project with CustomDbContext Implementation

    //the CustomDbContext needs a constructor that passes the options parameter to base DbContext class, note that the customDbContext is also being added at Program.cs to builder.Services.AddDbContext with options to enable a database provider in this example UseSqlServer, this is passed to base DbContext so it recognizes the database provider chosen (IE: MongoDb,SQL, etc).

    //with the steps above done, (ctrl shift b)build solution then: 

    Add-Migration MigrationName <--target project with CustomDbContext Implementation via package manager console 
    
    //all customDbContext classes properly implemented, and now in updated format, are "Migrated" or parsed out from the old out-of-date database into a new Migration folder in the project that has CustomDbContext implementation, a file named timestamp_MigrationName.cs with two methods, 
    //Up() which is called when updating the migration changes to existing database and Down() which is called when rolling back a migration, it simply drops the target tables, which should be rare. note that Up has content of the database entries, tables/collections, etc in plain text as stored in database being migrated ALL pre-existing data is parsed out into this new file, that outputs hardcoded chains of method calls to recreate the database with updated formatting.
    //at this point the updated database does not yet exist, all data is parsed out of old db format into new format, preped for the next step, note that changes should not be made in the generated migration file, instead files should be edited via the CustomDbContext,DTO and domain data models and their annotations and a new migration generated/prepped.

    //then run this package manager console command targeting solution project with customDbContext and the generated migration folder
    Update-Database -Verbose <--output verbose migration data in console, and executes the generated updated database to provider sql

    //after Update-Database, the generated migration code is executed which "migrates" the database content to the updated format based on the solution implementation. 
    //now the new tables/collections will be present in the target database name inside the database provider, in this case sql. also a new table is generated _EFMigrationsHistory to keep track of timestamp_customMigrationName performed.
    //the output database tables will have corresponding collumns for the udpated DbContext target Data models, key values, constraints triggers indexes and statistics, etc, which can be further configured after the migration takes place to safely update formatting changes in data models between server-database.


    //the data that will be in the database is the preexisting data in the previous format, being parsed into the new updated format. to simplify, say Person.cs has new Properties, instead of manually adding tables for these properties to each corresponding entry in the persons database, the migration updates all pre-existing entries filtered through to the new format, which then can be further configured or updated with corresponding key values, beyond default defined "seed" data values.
    //note that the migration generated timestamp_customMigrationName file inside the project contains ALL READABLE DATA from preexisting database parsed into the migration prep code file, so this file needs to be properly disposed of after a migration, and a migration should only EVER occur on a LIVE production database in a safe execution environment, containerized disposable cloud VM, so parsed out database data is not compromised during or after the migration process.
    //IDEALLY migrations take place using only placeholder/seed data in dev environment before any production takes place and stores actual user data, migration in dev environment can disregard the safety recommendation as no actual userdata would be exposed in any case.

    //to actually use the DbContext to interact with the database, see the CustomService class in this example Services.CountriesService and Services.PersonsService, they will inject the builder.Services added CustomDbContext at constructor and use it for CRUD operations with database
 */

namespace Entities
{
    //the customName-Database-Context object handles the serverside requests to and responses from the actual database system
    //the customName part identifies the database general content, so a "persons" database may have various collections/tables
    //pertaining to classes relevant to persons but not necessarily Person class properties,

    //note that DbContext class that customDbContext MUST inherit from, comes from Entityframeworkcore nuget package
    //there are variant package providers targeted at different databases like sql, mongodb etc,
    //add nuget package to project that has the DbContext classes, in this case CRUDxUnitExample.Entities

    public class PersonsDbContext : DbContext //EFCore uses Dependency Injection services by default, see Program.cs
    {
        //DbSet<T> sets a collection or table of T type expected in the database, not a runtime collection
        public DbSet<Person> Persons { get; set; }
        //while the DbContext is targeted at Person class, other class types may be relevant, here each person has a country as well
        public DbSet<Country> Countries { get; set; }

        public PersonsDbContext(DbContextOptions options) : base(options)
        {
            //note the constructor of the CustomDbContext is needed to pass the options parameters set at builder.Services.AddDbContext() in Program.cs
        }

        public List<Person> StoredProcedure_GetAllPersons()
        {
            List<Person> personsList = Persons.FromSqlRaw(@"EXECUTE [dbo].[GetAllPersons]").ToList();
            return personsList;
        }

        public int StoredProcedure_AddPerson(Person person)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PersonId", person.PersonId),
                new SqlParameter("@Name", person.Name),
                new SqlParameter("@Email", person.Email),
                new SqlParameter("@Dob", person.Dob),
                new SqlParameter("@Gender", person.Gender),
                new SqlParameter("@CountryId", person.CountryId),
                new SqlParameter("@Address", person.Address),
                new SqlParameter("@ReceiveNewsLetters", person.ReceiveNewsLetters),
                new SqlParameter("@CountryName", person.CountryName)
            };

            //Database is a predefined property of base DbContext class that CustomDbContext inherits, Database represents the actual database in runtime memory, it is NOT the external database provider itself
            //the ExecuteSqlRaw method returns an int to represent number of rows/stored entities affected. the EXECUTE call names the stored procedure, and @param reference names, followed by collection of SqlParameter KVPs from which the values are retrieved from runtime memory and sent to the database provider.
            return Database.ExecuteSqlRaw("EXECUTE [dbo].[AddPerson] @PersonId, @Name, @Email, @Dob, @Gender, @CountryId, @Address, @ReceveiNewsLetters, @CountryName", parameters);
            
            //foreach (PropertyInfo property in person.GetType().GetProperties())
            //{ 
            //}
        }

        //model creation happens and calls OnModedlCreating at some point when the db context is being instantiated
        //the DbSet<T> properties above are bound to corresponding table/collections through OnModelCreating() override
        //primary keys, table relations, seed logic various modifications can be made at model creation stage
        //the DbContext and these model bindings serve as a sort of Interface bridge between runtime memory and the data storage database system at the other end of the connection string address
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //fetching mock seed data to initialize the database 
            //mockDataUtil is reading two mock/seed data .json files
            PersonsMockDataUtil mockDataUtil = new();

            //in this example EFCSQL provider/driver package is being used, so setting ToTable()
            //a non relational db like MongoDB would use ToCollection() perhaps?
            //the name of the table is the ref name of the DbSet property
            modelBuilder.Entity<Person>().ToTable("Persons");

            foreach (Person person in mockDataUtil.mockPersons)
            {
                //the HasData() method is used to introduce "seed/mock" data into the database to initialize the table/collection rows/files
                //in the right data model format, although no validation is being performed here, its assumed the Entity class is formatted
                //correctly, and that the data being introduced is in a valid format HasData is used only for seed data
                modelBuilder.Entity<Person>().HasData(person);
            }

            //FluentAPI
            //the FluentAPI methods of EFcore function almost like LINQ queries or "promise" methods in syntax, the changes to the stored relational database of target entity type, of property WHERE property is ExampleChangedProperty.
            //the column name type and default values are given in past tense "Has" named methods made to update values of columns in the database, to imply that target colum component has given value henceforth.
            //this is mentioned because by creating a migration to update the database with the line below, the existing Person objects stored in the database tables/collection will still have null values for their ExampleChangedProperty, since none was when the objects were created initially, BUT NEW INSTANCES of Person added will take on the predefined default value if none is given to ExampleChangedProperty
            //the changes added to modelbuilder always need to be migrated to actually take effect in database provider,
            //using the Has() methods SET/configures the sql database, these are different to HasCheckConstraint that instead checks the existing configuration
            modelBuilder.Entity<Person>().Property(temp => temp.TaxId)
                .HasColumnName("TaxId")
                .HasColumnType("varchar(9)")
                .HasDefaultValue("999999999");

            //the string name passed will set the name of the database-side generated table/collection
            modelBuilder.Entity<Country>().ToTable("Countries");
            
            foreach (Country country in mockDataUtil.mockCountries)
            {
                modelBuilder.Entity<Country>().HasData(country);
            }

            //using the FluentAPI methods below, the changes need to be sent via migration to the database and updated. note the output of Up method by migration is generated from the FluentAPI methods called on the target Data model type given as generic type below.
            //note how the Property method takes a lambda expression looking for the given property type in the domain data model, and the HasX() methods are updating the name, columnType and DefaultValues IN THE DATABASE, so in the domain data model the property still has the ExampleChangedProperty name in source code
            //the call below targets ExampleChangedProperty, which has to be present in the source code, and upon migration updated to TaxId, now once the sourcecode is updated as well, the line below will fail to compile as ExampleChangedProperty is not present in the Person data model, this makes clear the divisionbetween the source code and database provider
            //modelBuilder.Entity<Person>().Property(obj => obj.ExampleChangedProperty).HasColumnName("TaxId").HasColumnType("varchar(11)").HasDefaultValue("00000000000");

            //FluentAPI can be used to modify database provider side validation rules, .HasIndex().IsUnique() can he used to make sure every new entry is not currently present in other rows, thus unique, or HasCheckCounstraint() given a name and the validation check, here the length of [TaxId] == 11 passes validation, less or more will cause the insert/update operation to fail, and a value constraint check will not be relevant for a read operation
            //these changes to OnModelCreation of CustomDbContext require a Add-Migration and Update-Database calls to effectuate these updates to Person entity in database provider, the original name of the column is no longer acessible after updating it so its no longer ExampleChangedProperty in the database, now its TaxId, the HasColumnName() call above has changed the column to TaxId, so calling len([TaxId]) for HasCheckConstraint() is valid this is all obvious in a way, but worth pointing out that the code-first approach determines the database provider data, which has to be accounted for in subsequent use in runtime

            //AFTER the changes made to CustomDbContext OnModelCreation have been migrated and updated to database, now viewing the sql object the Constraints folder has the Check_TaxId length constraint/validation for TaxId property
            //database side validation is not strictly required for all operations, but a safety measure that can further secure data entry by enforcing schema constraints typechecks and so on, to ensure the requested operation is valid, althought validation should occur before data is ever entered into a database, so serverside and client side validation are more of a requirement
            modelBuilder.Entity<Person>().HasCheckConstraint("Check_TaxId", "len([TaxId]) = 11");

            //statement below creates an index for target property and IsUnique ensures new additions to database are not already present in database.
            //note that the Person.cs model still has ExampleChangedProperty, attempting to search for obj.TaxId below fails, because the source code does not have the target property, even though the database provider does. this would require updating the source code as well
            //modelBuilder.Entity<Person>().HasIndex(obj => obj.TaxId).IsUnique();

            //note that the above changes that add constraints, default values etc are only detected ONCE in the migration, and once updated to database, even though the lines of code continue present above, they are not detected as "changes" any longer after applied, and so are ignored in future migrations, the point is the source code has already been applied to the database provider structure.

            //table relations (optional) 
            //explicitly declaring the table relation of the navigationKey properties of classes is OPTIONAL if using the [ForeignKey] annotations
            //in the model classes's properties, declaring it in the modelBuilder as done below does not require the [ForeignKey] tag
            //there are multiple ways to stipulate the table relations, here in the CustomDbContext OnModelBuilder the Person entity is acessed and has the relation directly set using a "navigation property" of person, that has a Country type associated with it, so many persons have a CountryId key relation, and the Person class has the Country property. this is used by the database to identify the "foreignKey" annotation of the other related object in this case the Country related to Person, and inside the Country the CountryId class has [Key] annotation
            //recommended to use [ForeignKey] instead to allow migration to automate this step, more flexible with ongoing changes
            modelBuilder.Entity<Person>(person =>
            {
                //fetched the Person entity, and for each person set ONE country relation that has key of Country class, and the Country class has many persons
                //important to remember that the Country object and the Persons objects are navigation properties, so they will only be populated/retrieved if specified
                //important to note the HasForeignKey method call, it is stipulating that each person's CountryId property is the key that should be used to search for the related Country, this means the Person class NEEDS TO HAVE a property that hold the actual key of related Country, that has the same [Key]CountryId, this is used by the database to identify the "foreignKey" of the other related object
                person.HasOne<Country>(withKey => withKey.Country).WithMany(p => p.Persons).HasForeignKey(person => person.CountryId);
            });

            //the modelBuilder param is passed to the base/parent DbContext class with the added bindings
            //the operations set to modelBuilder, like the changed to domain data models, will be detected and define changes the migration prepares for the database
            base.OnModelCreating(modelBuilder);
    }
}
public class PersonsMockDataUtil
{
    public List<Person> mockPersons;
        public List<Country> mockCountries;

        //initializing some empty lists
        public PersonsMockDataUtil()
        {
            mockPersons = new List<Person>();
            mockCountries = new List<Country>();
            FetchMockData();
        }
        private void FetchMockData()
        {
            if (mockCountries.Count == 0)
            {
                //once FetchMockData is called the placeholder data is being retrieved from the json files just for demonstration
                //the json data could be from an external API source, from user clients, etc.
                string countriesJson = System.IO.File.ReadAllText("mockCountries.json");
                mockCountries = System.Text.Json.JsonSerializer.Deserialize<List<Country>>(countriesJson);
            }

            if (mockPersons.Count == 0)
            {
                string personsJson = System.IO.File.ReadAllText("mockPersons.json");
                mockPersons = System.Text.Json.JsonSerializer.Deserialize<List<Person>>(personsJson);
            }
        }
    }
}
