using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddPerson_StoredProcedure_Updated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Updating the AddPerson StoredProcedure to include new property
            //note the data types are badly set still, for example nvarchar(MAX) is inadvisable as specialcharacters, numbers, letters, symbols etc with no string length limit can go into that property. this can be problematic especially if the field receives some user input, limiting the length and expected type of data is needed, see CustomDbContext OnModelBuilder method
            string storedProcedure_AddPerson = @"
            ALTER PROCEDURE [dbo].[AddPerson]
                @PersonId uniqueidentifier,
                @Name nvarchar(MAX),
                @Email nvarchar(MAX),
                @Dob datetime2(7),
                @Gender nvarchar(MAX),
                @CountryId uniqueidentifier,
                @Address nvarchar(MAX),
                @ReceiveNewsLetters bit,
                @CountryName nvarchar(MAX),
                @ExampleChangedProperty nvarchar(MAX)
            AS BEGIN
                SET NOCOUNT ON;
                BEGIN TRY
                    INSERT INTO [dbo].[Persons](PersonId, Name, Email, Dob, Gender, CountryId, Address, ReceiveNewsLetters, CountryName, ExampleChangedProperty)
                    VALUES (@PersonId, @Name, @Email, @Dob, @Gender, @CountryId, @Address, @ReceiveNewsLetters, @CountryName, @ExampleChangedProperty);
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
