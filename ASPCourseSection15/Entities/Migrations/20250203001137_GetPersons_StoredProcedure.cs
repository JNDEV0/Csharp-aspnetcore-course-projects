/*
 the solution needs to build before generating the migration file, in this example MockDataUtil and CRUDTests had to be updated to to use a CustomDbContext instead of mockdata, but notice how the migration generated HAS NO UPDATES because the CustomDbContext class is the one being evaluated on the migration, pointing this out to enphasize what the migration is about

note that the "default project" referenced, Entities in package manager console setting has the CustomDbContext class, but has no definition for any method related to the stored procedure yet. this is intentional initially as adding the migration without changes will generate the empty migration timestamp_customMigrationName file, which then can be manually edited to insert or drop the storedProcedure, run in package mngr console:
    Add-Migration GetPersons_StoredProcedure

after adding the migration that has no changes to CustomDbContext, this file is created for adding the custom stored procedure, the up method will insert the stored procedure, the down method will drop the stored procedure from the database, this is a good way of organizing and managing the procedures in the database without manually editing the database, and the migration can be rolled back to remove the procedure.

note that this stored procedure concept is not solution/project, table/collection or even database provider specific, a stored procedure enables shorter messages to the database from server, storing the complex multi step operation at the database instead of transmitting the entire operation string each time, like a server->db method call. the custom procedure operation may differ for different database providers like mongodb, firebase, etc.

in this example the string passed to the target DbSet is plain SQL, calling the db provider to EXECUTE the stored procedure that has the stored SQL code that was added to the database via migration update. the return of FromSqlRaw is IQueryable<T>, which can be converted to List<T> for example.

in this example two methods are needed, 
at CustomDbContext a method is needed later to actually call the StoredProcedure being added in this migration the DbContext has a method defined that sends an EXECUTE command to the database provider after the stored procedure is inserted to the database by implementing the custom procedure code calling via package manager console: 
    Update-Database

at customService create a method to call the CustomDbContext's method that sends the Stored Procedure call to database provider, receives and relays the async/await response from db provider back to customService to use the data in runtime operations, ie: find object by property value, filtered list of objects, update/create confirmation etc

after the dbprovider is updated in this example can see at SQL Database through View > SQL Server Object Explorer > sql server, db, persons collection programability folder.

read operations return and use:
IQueryable<Model> DbSetName.FromSqlRaw(
string sql,
params object[] parameters)
 */
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class GetPersons_StoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //in SQL, the first line CREATE... tells db provider where to store under what name
            //between as begin and end goes the actual sql string to execute
            //in this migration, a simple READ, get/SELECT all these column values from Persons rows/table/collection
            //note that in READ type operation here the return value is a collection of resulting output objects
            //in CUD type operations the response may be an int that represents the number of objects Created/Update/Deleted.
            string storedProcedure_GetAllPersons = @"
                CREATE PROCEDURE [dbo].[GetAllPersons]
                AS BEGIN
                   SELECT PersonId, Name, Email, Dob, Gender, CountryId, Address, ReceiveNewsletters, CountryName FROM [dbo].[Persons]
                END
            ";
            migrationBuilder.Sql(storedProcedure_GetAllPersons);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //the Down method is called incase the procedure is rolled back using Remove-Migration, in this case the above procedure wouldve already been migrated to the database, in case it needs to be removed from database provider
            string storedProcedure_GetAllPersons = @"
                DROP PROCEDURE [dbo].[GetAllPersons]
            ";
            migrationBuilder.Sql(storedProcedure_GetAllPersons);
        }
    }
}
