using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetCare360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_USUARIO_PETCARE",
                columns: table => new
                {
                    ID_USUARIO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NM_USUARIO = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    SENHA_HASH = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: false),
                    PERFIL = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    DT_CRIACAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_USUARIO_PETCARE", x => x.ID_USUARIO);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_USUARIO_PETCARE_EMAIL",
                table: "TB_USUARIO_PETCARE",
                column: "EMAIL",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_USUARIO_PETCARE");
        }
    }
}
