//this migration handles changes to the ExampleChangedPRoperty, renaming it in the database as TaxId and defining some constraints,
//see PersonDbContext OnModelCreation method call.
//the FluentAPI methods of EFcore function almost like LINQ queries or "promise" methods in syntax, the changes to the stored relational database of target entity type, of property WHERE property is ExampleChangedProperty.

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class Person_ModelUpdate_ExampleChangedPropertyToTaxId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExampleChangedProperty",
                table: "Persons",
                newName: "TaxId");

            migrationBuilder.AlterColumn<string>(
                name: "TaxId",
                table: "Persons",
                type: "varchar(11)",
                nullable: true,
                defaultValue: "00000000000",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "PersonId",
                keyValue: new Guid("c03bbe45-9aeb-4d24-99e0-4743016ffce9"),
                column: "TaxId",
                value: "00000000000");

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "PersonId",
                keyValue: new Guid("c3abddbd-cf50-41d2-b6c4-cc7d5a750928"),
                column: "TaxId",
                value: "00000000000");

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "PersonId",
                keyValue: new Guid("c6d50a47-f7e6-4482-8be0-4ddfc057fa6e"),
                column: "TaxId",
                value: "00000000000");

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "PersonId",
                keyValue: new Guid("d15c6d9f-70b4-48c5-afd3-e71261f1a9be"),
                column: "TaxId",
                value: "00000000000");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaxId",
                table: "Persons",
                newName: "ExampleChangedProperty");

            migrationBuilder.AlterColumn<string>(
                name: "ExampleChangedProperty",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(11)",
                oldNullable: true,
                oldDefaultValue: "00000000000");

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "PersonId",
                keyValue: new Guid("c03bbe45-9aeb-4d24-99e0-4743016ffce9"),
                column: "ExampleChangedProperty",
                value: null);

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "PersonId",
                keyValue: new Guid("c3abddbd-cf50-41d2-b6c4-cc7d5a750928"),
                column: "ExampleChangedProperty",
                value: null);

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "PersonId",
                keyValue: new Guid("c6d50a47-f7e6-4482-8be0-4ddfc057fa6e"),
                column: "ExampleChangedProperty",
                value: null);

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "PersonId",
                keyValue: new Guid("d15c6d9f-70b4-48c5-afd3-e71261f1a9be"),
                column: "ExampleChangedProperty",
                value: null);
        }
    }
}
