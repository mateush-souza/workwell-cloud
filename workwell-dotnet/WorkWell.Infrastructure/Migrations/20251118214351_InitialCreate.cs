using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkWell.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EMPRESAS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NOME = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CNPJ = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    SETOR = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DATA_CADASTRO = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMPRESAS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DEPARTAMENTOS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NOME = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DESCRICAO = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EMPRESA_ID = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DEPARTAMENTOS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DEPARTAMENTOS_EMPRESAS_EMPRESA_ID",
                        column: x => x.EMPRESA_ID,
                        principalTable: "EMPRESAS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "USUARIOS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NOME = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EMAIL = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SENHA_HASH = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EMPRESA_ID = table.Column<int>(type: "int", nullable: false),
                    DEPARTAMENTO_ID = table.Column<int>(type: "int", nullable: true),
                    CARGO = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ROLE = table.Column<int>(type: "int", nullable: false),
                    ATIVO = table.Column<bool>(type: "bit", nullable: false),
                    DATA_ULTIMO_ACESSO = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIOS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_USUARIOS_DEPARTAMENTOS_DEPARTAMENTO_ID",
                        column: x => x.DEPARTAMENTO_ID,
                        principalTable: "DEPARTAMENTOS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_USUARIOS_EMPRESAS_EMPRESA_ID",
                        column: x => x.EMPRESA_ID,
                        principalTable: "EMPRESAS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ALERTAS_BURNOUT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    USUARIO_ID = table.Column<int>(type: "int", nullable: false),
                    DATA_ALERTA = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NIVEL_RISCO = table.Column<int>(type: "int", nullable: false),
                    SCORE_RISCO = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    DESCRICAO = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RECOMENDACOES = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LIDO = table.Column<bool>(type: "bit", nullable: false),
                    DATA_LEITURA = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ALERTAS_BURNOUT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ALERTAS_BURNOUT_USUARIOS_USUARIO_ID",
                        column: x => x.USUARIO_ID,
                        principalTable: "USUARIOS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CHECKINS_DIARIOS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    USUARIO_ID = table.Column<int>(type: "int", nullable: false),
                    DATA_CHECKIN = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NIVEL_STRESS = table.Column<int>(type: "int", nullable: false),
                    HORAS_TRABALHADAS = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    HORAS_SONO = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    SENTIMENTO = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OBSERVACOES = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SCORE_BEMESTAR = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHECKINS_DIARIOS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CHECKINS_DIARIOS_USUARIOS_USUARIO_ID",
                        column: x => x.USUARIO_ID,
                        principalTable: "USUARIOS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "METRICAS_SAUDE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    USUARIO_ID = table.Column<int>(type: "int", nullable: false),
                    DATA_REGISTRO = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QUALIDADE_SONO = table.Column<int>(type: "int", nullable: true),
                    MINUTOS_ATIVIDADE_FISICA = table.Column<int>(type: "int", nullable: true),
                    LITROS_AGUA = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    FREQUENCIA_CARDIACA = table.Column<int>(type: "int", nullable: true),
                    PASSOS_DIARIOS = table.Column<int>(type: "int", nullable: true),
                    PESO_KG = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_METRICAS_SAUDE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_METRICAS_SAUDE_USUARIOS_USUARIO_ID",
                        column: x => x.USUARIO_ID,
                        principalTable: "USUARIOS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ALERTAS_BURNOUT_LIDO",
                table: "ALERTAS_BURNOUT",
                column: "LIDO");

            migrationBuilder.CreateIndex(
                name: "IX_ALERTAS_BURNOUT_NIVEL_RISCO",
                table: "ALERTAS_BURNOUT",
                column: "NIVEL_RISCO");

            migrationBuilder.CreateIndex(
                name: "IX_ALERTAS_BURNOUT_USUARIO_ID",
                table: "ALERTAS_BURNOUT",
                column: "USUARIO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CHECKINS_DIARIOS_USUARIO_ID_DATA_CHECKIN",
                table: "CHECKINS_DIARIOS",
                columns: new[] { "USUARIO_ID", "DATA_CHECKIN" });

            migrationBuilder.CreateIndex(
                name: "IX_DEPARTAMENTOS_EMPRESA_ID",
                table: "DEPARTAMENTOS",
                column: "EMPRESA_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMPRESAS_CNPJ",
                table: "EMPRESAS",
                column: "CNPJ",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_METRICAS_SAUDE_USUARIO_ID_DATA_REGISTRO",
                table: "METRICAS_SAUDE",
                columns: new[] { "USUARIO_ID", "DATA_REGISTRO" });

            migrationBuilder.CreateIndex(
                name: "IX_USUARIOS_DEPARTAMENTO_ID",
                table: "USUARIOS",
                column: "DEPARTAMENTO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_USUARIOS_EMAIL",
                table: "USUARIOS",
                column: "EMAIL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USUARIOS_EMPRESA_ID",
                table: "USUARIOS",
                column: "EMPRESA_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ALERTAS_BURNOUT");

            migrationBuilder.DropTable(
                name: "CHECKINS_DIARIOS");

            migrationBuilder.DropTable(
                name: "METRICAS_SAUDE");

            migrationBuilder.DropTable(
                name: "USUARIOS");

            migrationBuilder.DropTable(
                name: "DEPARTAMENTOS");

            migrationBuilder.DropTable(
                name: "EMPRESAS");
        }
    }
}
