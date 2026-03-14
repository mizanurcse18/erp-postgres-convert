using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class divisionidchangestointeger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, update existing text values to integers using CAST
            migrationBuilder.Sql(@"
                -- Convert division_id from text to integer
                ALTER TABLE department 
                ALTER COLUMN division_id TYPE integer 
                USING CASE 
                    WHEN division_id IS NULL OR division_id = '' THEN 0
                    ELSE division_id::integer
                END;
                
                -- Set column to NOT NULL
                ALTER TABLE department 
                ALTER COLUMN division_id SET NOT NULL;
                
                -- Set default value
                ALTER TABLE department 
                ALTER COLUMN division_id SET DEFAULT 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Convert back from integer to text
            migrationBuilder.Sql(@"
                ALTER TABLE department 
                ALTER COLUMN division_id TYPE text 
                USING division_id::text;
                
                ALTER TABLE department 
                ALTER COLUMN division_id DROP NOT NULL;
                
                ALTER TABLE department 
                ALTER COLUMN division_id DROP DEFAULT;
            ");
        }
    }
}
