using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixIncomeUserRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incomes_Users_UserId1",
                table: "Incomes");

            migrationBuilder.DropIndex(
                name: "IX_Incomes_UserId1",
                table: "Incomes");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Incomes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "Incomes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Incomes_UserId1",
                table: "Incomes",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Incomes_Users_UserId1",
                table: "Incomes",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
