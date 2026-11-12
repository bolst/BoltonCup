using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Persistence.Migrations.Auth
{
    /// <inheritdoc />
    public partial class DropUserAccountId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hand-written: copy links the old code made after MoveUserLinkToAccounts ran, refuse to drop any it can't
            // carry over, then drop the column. Running this before MoveUserLinkToAccounts would drop every link, so it
            // refuses that too.
            migrationBuilder.Sql("""
                DO $$
                DECLARE
                    lost_links text;
                BEGIN
                    IF to_regclass('core.accounts') IS NULL THEN
                        RETURN;
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'core' AND table_name = 'accounts' AND column_name = 'user_id') THEN
                        RAISE EXCEPTION 'core.accounts has no user_id yet. Apply the core migrations (./update_db.sh) first, then rerun.';
                    END IF;

                    UPDATE core.accounts a
                    SET user_id = u."Id"
                    FROM auth."AspNetUsers" u
                    WHERE u."AccountId" = a.id
                      AND a.user_id IS NULL
                      AND NOT EXISTS (SELECT 1 FROM core.accounts owned WHERE owned.user_id = u."Id");

                    SELECT string_agg(format('user %s: account %s', u."Id", u."AccountId"), '; ')
                    INTO lost_links
                    FROM auth."AspNetUsers" u
                    JOIN core.accounts a ON a.id = u."AccountId"
                    WHERE a.user_id IS DISTINCT FROM u."Id";
                    IF lost_links IS NOT NULL THEN
                        RAISE EXCEPTION 'Links that would be lost (the account or the user is already linked elsewhere). Resolve them, then rerun: %', lost_links;
                    END IF;
                END $$;
                """);

            migrationBuilder.DropColumn(
                name: "AccountId",
                schema: "auth",
                table: "AspNetUsers");

            // Hand-written: a deleted user frees their account, and user_id can only name a real user. EF can't model a
            // key into the auth schema. Whichever of this and MoveUserLinkToAccounts runs second adds it.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF to_regclass('auth."AspNetUsers"') IS NULL
                        OR NOT EXISTS (
                            SELECT 1 FROM information_schema.columns
                            WHERE table_schema = 'core' AND table_name = 'accounts' AND column_name = 'user_id')
                        OR EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_accounts_AspNetUsers_user_id') THEN
                        RETURN;
                    END IF;

                    ALTER TABLE core.accounts
                        ADD CONSTRAINT "FK_accounts_AspNetUsers_user_id"
                        FOREIGN KEY (user_id) REFERENCES auth."AspNetUsers" ("Id") ON DELETE SET NULL;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                schema: "auth",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            // Hand-written: restore the links so code that reads this column works again. Without user_id, the core
            // migration's Down has already copied them back.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'core' AND table_name = 'accounts' AND column_name = 'user_id') THEN
                        RETURN;
                    END IF;

                    UPDATE auth."AspNetUsers" u
                    SET "AccountId" = a.id
                    FROM core.accounts a
                    WHERE a.user_id = u."Id";
                END $$;
                """);
        }
    }
}