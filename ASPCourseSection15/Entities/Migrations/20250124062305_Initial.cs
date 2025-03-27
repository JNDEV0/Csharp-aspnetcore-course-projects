/*
 Install-Package Microsoft.EntityFrameworkCore.Toolsinstalls the package that constains the migration functionality.migration is useful when there are changes in the data strcuture of a table or collection, to update all the files, the old formatted files are parsed in from the old table/collection, and updated to new format,

the target DbContext that is being added at Program.cs with builder options to enable a database provider, in this example UseSqlServer, needs to be passed to the customDbContext constructor to receive the injected options as DbContextOptions passed to base DbContext class, with this done ctrl shift b or build solution then run command Add-Migration Initial via package manager console all customDbContext classes properly implemented are "Migrated" to a new Migration folder in the solution

once previous steps are performed, Add-Migration has added a timestamped_Initial.csclass that contains generated code to create the new adjusted tables, running Update-Database

for writing Stored procedured into the empty generated migration (if migration is not empty, there are other changes detected, migrate those first and then start a new migration to containerize concerns)

Create/Update/Delete operations:
int DbContext.Database.ExecuteSqlRaw(
    string sql, //"EXECUTE [dbo].[StoredProcedureName] @param1 @param2"
    params object[] parameters) //object array collection of parameter value refs

read operations:
IQueryable<Model> DbSetName.FromSqlRaw(
string sql,
params object[] parameters)
 */
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.CountryId);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dob = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiveNewsLetters = table.Column<bool>(type: "bit", nullable: false),
                    CountryName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.PersonId);
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "CountryId", "Name" },
                values: new object[,]
                {
                    { new Guid("14e0b461-8fb4-4e23-b853-306fac4429bf"), "Mongolia" },
                    { new Guid("350d4004-f8cd-42c0-9eb2-4ec6a8ec0314"), "Uzbekistan" }
                });

            migrationBuilder.InsertData(
                table: "Persons",
                columns: new[] { "PersonId", "Address", "CountryId", "CountryName", "Dob", "Email", "Gender", "Name", "ReceiveNewsLetters" },
                values: new object[,]
                {
                    { new Guid("c03bbe45-9aeb-4d24-99e0-4743016ffce9"), "123 trueman rd", new Guid("14e0b461-8fb4-4e23-b853-306fac4429bf"), "Mongolia", new DateTime(1989, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "email1@format.com", "Male", "Borat", false },
                    { new Guid("c3abddbd-cf50-41d2-b6c4-cc7d5a750928"), "43 sea st", new Guid("14e0b461-8fb4-4e23-b853-306fac4429bf"), "Mongolia", new DateTime(1980, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "email2@format.com", "Male", "Zappa", false },
                    { new Guid("c6d50a47-f7e6-4482-8be0-4ddfc057fa6e"), "99 longway ct", new Guid("350d4004-f8cd-42c0-9eb2-4ec6a8ec0314"), "Uzbekistan", new DateTime(1974, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "email3@format.com", "Female", "Stern", true },
                    { new Guid("d15c6d9f-70b4-48c5-afd3-e71261f1a9be"), "44 construct dr", new Guid("350d4004-f8cd-42c0-9eb2-4ec6a8ec0314"), "Uzbekistan", new DateTime(1960, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "email4@format.com", "Other", "Bean", true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "Persons");
        }
    }
}
