using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DamageAtSlotLevels",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    _0 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    _1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    _2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    _3 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    _4 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    _5 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    _6 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    _7 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    _8 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    _9 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DamageAtSlotLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DamageTypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DamageTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Damages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DamageTypeId = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    DamageAtSlotLevelId = table.Column<string>(type: "nvarchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Damages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Damages_DamageAtSlotLevels_DamageAtSlotLevelId",
                        column: x => x.DamageAtSlotLevelId,
                        principalTable: "DamageAtSlotLevels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Damages_DamageTypes_DamageTypeId",
                        column: x => x.DamageTypeId,
                        principalTable: "DamageTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Spells",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Desc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HigherLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Range = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Components = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Material = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    Ritual = table.Column<bool>(type: "bit", nullable: false),
                    Duration = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Concentration = table.Column<bool>(type: "bit", nullable: false),
                    CastingTime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    AttackType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DamageId = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    SchoolId = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spells_Damages_DamageId",
                        column: x => x.DamageId,
                        principalTable: "Damages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Spells_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClassSpell",
                columns: table => new
                {
                    ClassesId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    SpellsId = table.Column<string>(type: "nvarchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSpell", x => new { x.ClassesId, x.SpellsId });
                    table.ForeignKey(
                        name: "FK_ClassSpell_Classes_ClassesId",
                        column: x => x.ClassesId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassSpell_Spells_SpellsId",
                        column: x => x.SpellsId,
                        principalTable: "Spells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subclasses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SpellId = table.Column<string>(type: "nvarchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subclasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subclasses_Spells_SpellId",
                        column: x => x.SpellId,
                        principalTable: "Spells",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassSpell_SpellsId",
                table: "ClassSpell",
                column: "SpellsId");

            migrationBuilder.CreateIndex(
                name: "IX_Damages_DamageAtSlotLevelId",
                table: "Damages",
                column: "DamageAtSlotLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Damages_DamageTypeId",
                table: "Damages",
                column: "DamageTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Spells_DamageId",
                table: "Spells",
                column: "DamageId");

            migrationBuilder.CreateIndex(
                name: "IX_Spells_SchoolId",
                table: "Spells",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Subclasses_SpellId",
                table: "Subclasses",
                column: "SpellId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassSpell");

            migrationBuilder.DropTable(
                name: "Subclasses");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Spells");

            migrationBuilder.DropTable(
                name: "Damages");

            migrationBuilder.DropTable(
                name: "Schools");

            migrationBuilder.DropTable(
                name: "DamageAtSlotLevels");

            migrationBuilder.DropTable(
                name: "DamageTypes");
        }
    }
}
