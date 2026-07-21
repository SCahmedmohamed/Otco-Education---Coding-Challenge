using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgramDesigner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FirstMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PrerequisiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    Rule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PickCount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nodes_Nodes_ParentGroupId",
                        column: x => x.ParentGroupId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nodes_Nodes_PrerequisiteId",
                        column: x => x.PrerequisiteId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Programs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RootGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Programs_Nodes_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Nodes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Programs_Nodes_RootGroupId",
                        column: x => x.RootGroupId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_ParentGroupId",
                table: "Nodes",
                column: "ParentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_PrerequisiteId",
                table: "Nodes",
                column: "PrerequisiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_ProgramId",
                table: "Nodes",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Programs_GroupId",
                table: "Programs",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Programs_RootGroupId",
                table: "Programs",
                column: "RootGroupId",
                unique: true,
                filter: "[RootGroupId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_Programs_ProgramId",
                table: "Nodes",
                column: "ProgramId",
                principalTable: "Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_Programs_ProgramId",
                table: "Nodes");

            migrationBuilder.DropTable(
                name: "Programs");

            migrationBuilder.DropTable(
                name: "Nodes");
        }
    }
}
