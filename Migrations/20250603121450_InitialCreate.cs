using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ShredleApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "solos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    artist = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    spotify_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    start_time_clip1 = table.Column<double>(type: "double precision", nullable: false),
                    end_time_clip1 = table.Column<double>(type: "double precision", nullable: false),
                    start_time_clip2 = table.Column<double>(type: "double precision", nullable: false),
                    end_time_clip2 = table.Column<double>(type: "double precision", nullable: false),
                    start_time_clip3 = table.Column<double>(type: "double precision", nullable: false),
                    end_time_clip3 = table.Column<double>(type: "double precision", nullable: false),
                    start_time_clip4 = table.Column<double>(type: "double precision", nullable: false),
                    end_time_clip4 = table.Column<double>(type: "double precision", nullable: false),
                    guitarist = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    hint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "games",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    solo_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_games", x => x.id);
                    table.ForeignKey(
                        name: "FK_games_solos_solo_id",
                        column: x => x.solo_id,
                        principalTable: "solos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_games_date",
                table: "games",
                column: "date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_games_solo_id",
                table: "games",
                column: "solo_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "games");

            migrationBuilder.DropTable(
                name: "solos");
        }
    }
}
