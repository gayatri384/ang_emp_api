using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ang_emp_api.Migrations
{
    /// <inheritdoc />
    public partial class assetsthree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConditionOnAssign",
                table: "AssetAssignments");

            migrationBuilder.RenameColumn(
                name: "AssetType",
                table: "Assets",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Assets",
                newName: "AssetId");

            migrationBuilder.RenameColumn(
                name: "IsReturned",
                table: "AssetAssignments",
                newName: "IsDamagedOnReturn");

            migrationBuilder.RenameColumn(
                name: "ConditionOnReturn",
                table: "AssetAssignments",
                newName: "Remarks");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AssetAssignments",
                newName: "AssignmentId");

            migrationBuilder.AddColumn<bool>(
                name: "IsDamaged",
                table: "Assets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PurchaseDate",
                table: "Assets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AssetAssignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDamaged",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "PurchaseDate",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AssetAssignments");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "Assets",
                newName: "AssetType");

            migrationBuilder.RenameColumn(
                name: "AssetId",
                table: "Assets",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "Remarks",
                table: "AssetAssignments",
                newName: "ConditionOnReturn");

            migrationBuilder.RenameColumn(
                name: "IsDamagedOnReturn",
                table: "AssetAssignments",
                newName: "IsReturned");

            migrationBuilder.RenameColumn(
                name: "AssignmentId",
                table: "AssetAssignments",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "ConditionOnAssign",
                table: "AssetAssignments",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
