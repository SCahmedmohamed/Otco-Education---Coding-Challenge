using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgramDesigner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CheckNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Programs_Nodes_GroupId",
                table: "Programs");

            migrationBuilder.DropIndex(
                name: "IX_Programs_GroupId",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Programs");

            migrationBuilder.AlterColumn<Guid>(
                name: "PrerequisiteId",
                table: "Nodes",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "Programs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PrerequisiteId",
                table: "Nodes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Programs_GroupId",
                table: "Programs",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Programs_Nodes_GroupId",
                table: "Programs",
                column: "GroupId",
                principalTable: "Nodes",
                principalColumn: "Id");
        }
    }
}
