using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddPerson_StoredProcedure_Edit : Migration
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
