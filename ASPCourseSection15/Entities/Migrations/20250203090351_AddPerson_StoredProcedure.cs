/*
 same step as previous migrations: 
Add-Migration StoredProcedure_AddPerson

execute in package manager console to generate an empty migration given there are no changes to the DbSet data structures.

this is done to create the timestamp_customMigraitonName file that can be edited to provide the custom database procedure, when the migraiton is Updated the procedures will be callable by name in the database instead of having to pass the custom procedure string with each request, just the name of the procedure is passed like a method call to database provider
 */
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddPerson_StoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //procedure with parameters
            //note the procedure first defines a name [dbo].[AddPerson] then the datatypes/lengths are defined to each parameter and the table columns/keys to insert into and their @values.
            //more complex operations are possible depending on db provider capabilities, like validating the data types in the database, pipelines etc
            string storedProcedure_AddPerson = @"
            CREATE PROCEDURE [dbo].[AddPerson]
                @PersonId uniqueidentifier,
                @Name nvarchar(MAX),
                @Email nvarchar(MAX),
                @Dob datetime2(7),
                @Gender nvarchar(MAX),
                @CountryId uniqueidentifier,
                @Address nvarchar(MAX),
                @ReceiveNewsLetters bit,
                @CountryName nvarchar(MAX)
            AS
            BEGIN
                SET NOCOUNT ON;
                BEGIN TRY
                    INSERT INTO [dbo].[Persons](PersonId, Name, Email, Dob, Gender, CountryId, Address, ReceiveNewsLetters, CountryName)
                    VALUES (@PersonId, @Name, @Email, @Dob, @Gender, @CountryId, @Address, @ReceiveNewsLetters, @CountryName);
                END TRY
                BEGIN CATCH
                    -- Handle error
                    SELECT ERROR_MESSAGE() AS ErrorMessage;
                END CATCH;
            END;
            ";

            migrationBuilder.Sql(storedProcedure_AddPerson);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string storedProcedure_AddPerson = @"
                DROP PROCEDURE [dbo].[AddPerson]
            ";
            migrationBuilder.Sql(storedProcedure_AddPerson);
        }
    }
}
