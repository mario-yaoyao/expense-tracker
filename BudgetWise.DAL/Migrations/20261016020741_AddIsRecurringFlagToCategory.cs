using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetWise.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRecurringFlagToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "Categories");
        }
    }
}
