//this migration is intended to update the stored procedure in the database to reflect the new property that was previously updated, ExampleChangedProperty see Person.cs class
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class GetPersons_StoredProcedure_Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string storedProcedure_GetAllPersons = @"
                ALTER PROCEDURE [dbo].[GetAllPersons]
                AS BEGIN
                   SELECT PersonId, Name, Email, Dob, Gender, CountryId, Address, ReceiveNewsletters, CountryName, ExampleChangedProperty FROM [dbo].[Persons]
                END
            ";
            migrationBuilder.Sql(storedProcedure_GetAllPersons);
        }
        
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string storedProcedure_GetAllPersons = @"
                DROP PROCEDURE [dbo].[GetAllPersons]
            ";
            migrationBuilder.Sql(storedProcedure_GetAllPersons);
        }
    }
}
