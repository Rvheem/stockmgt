using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StockManagementApp.Models; // Add missing namespace

#nullable disable

namespace StockManagementApp.Migrations
{
    [DbContext(typeof(StockContext))]
    [Migration("20250530023830_FixForeignKeyIssue")]
    partial class FixForeignKeyIssue : Migration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            // Your model definitions will be here
        }
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Implementation of your migration
            // This is required for the Migration abstract class
        }
        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback logic for your migration
            // This is required for the Migration abstract class
        }
    }
}