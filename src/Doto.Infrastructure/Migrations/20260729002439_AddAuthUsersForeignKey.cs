using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Doto.Infrastructure.Migrations
{
    public partial class AddAuthUsersForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE public.app_users
                ADD CONSTRAINT fk_app_users_auth_users
                FOREIGN KEY (id) REFERENCES auth.users (id)
                ON DELETE NO ACTION;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE public.app_users DROP CONSTRAINT IF EXISTS fk_app_users_auth_users;");
        }
    }
}
