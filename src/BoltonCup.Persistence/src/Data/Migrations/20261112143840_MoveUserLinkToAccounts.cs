using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoltonCup.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveUserLinkToAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "user_id",
                schema: "core",
                table: "accounts",
                type: "text",
                nullable: true);

            // Hand-written backfill: copy each user's link onto the account. UPDATE ... FROM would keep an arbitrary
            // user when two share an account, so refuse instead and name them. Skipped on a database without auth.
            migrationBuilder.Sql("""
                DO $$
                DECLARE
                    shared_accounts text;
                BEGIN
                    IF to_regclass('auth."AspNetUsers"') IS NULL THEN
                        RETURN;
                    END IF;

                    SELECT string_agg(format('account %s: users %s', "AccountId", user_ids), '; ')
                    INTO shared_accounts
                    FROM (
                        SELECT "AccountId", string_agg("Id", ', ') AS user_ids
                        FROM auth."AspNetUsers"
                        WHERE "AccountId" IS NOT NULL
                        GROUP BY "AccountId"
                        HAVING count(*) > 1
                    ) shared;
                    IF shared_accounts IS NOT NULL THEN
                        RAISE EXCEPTION 'Accounts linked to more than one user. Unlink all but one, then rerun: %', shared_accounts;
                    END IF;

                    UPDATE core.accounts a
                    SET user_id = u."Id"
                    FROM auth."AspNetUsers" u
                    WHERE u."AccountId" = a.id;
                END $$;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_accounts_user_id",
                schema: "core",
                table: "accounts",
                column: "user_id",
                unique: true);

            // Hand-written: a deleted user frees their account, and user_id can only name a real user. EF can't model a
            // key into the auth schema. Whichever of this and DropUserAccountId runs second adds it.
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
            // Hand-written: new code links only through user_id, so copy those links back for the old code. If
            // DropUserAccountId is still applied there is nowhere to copy them, so refuse rather than drop them.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'auth' AND table_name = 'AspNetUsers' AND column_name = 'AccountId') THEN
                        IF EXISTS (SELECT 1 FROM core.accounts WHERE user_id IS NOT NULL) THEN
                            RAISE EXCEPTION 'auth."AspNetUsers" has no AccountId to copy links back to. Roll back the auth migration DropUserAccountId first, then rerun.';
                        END IF;

                        RETURN;
                    END IF;

                    UPDATE auth."AspNetUsers" u
                    SET "AccountId" = a.id
                    FROM core.accounts a
                    WHERE a.user_id = u."Id";
                END $$;
                """);

            migrationBuilder.DropIndex(
                name: "IX_accounts_user_id",
                schema: "core",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "user_id",
                schema: "core",
                table: "accounts");
        }
    }
}