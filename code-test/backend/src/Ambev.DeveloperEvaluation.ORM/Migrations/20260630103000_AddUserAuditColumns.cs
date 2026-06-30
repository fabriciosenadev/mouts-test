using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ambev.DeveloperEvaluation.ORM.Migrations;

[DbContext(typeof(DefaultContext))]
[Migration("20260630103000_AddUserAuditColumns")]
public class AddUserAuditColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE "Users"
            ADD COLUMN IF NOT EXISTS "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
            """);

        migrationBuilder.Sql(
            """
            ALTER TABLE "Users"
            ADD COLUMN IF NOT EXISTS "UpdatedAt" timestamp with time zone NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""ALTER TABLE "Users" DROP COLUMN IF EXISTS "CreatedAt";""");
        migrationBuilder.Sql("""ALTER TABLE "Users" DROP COLUMN IF EXISTS "UpdatedAt";""");
    }
}
