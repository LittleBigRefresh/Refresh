using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20260527172459_DisallowEntitiesCaseInsensitively")]
    public partial class DisallowEntitiesCaseInsensitively : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove case-insensitively duplicate entries before lowercasing primary keys to not cause duplicate keys
            // Email Addresses
            migrationBuilder.Sql
            ("""
                DELETE FROM "DisallowedEmailAddresses"
                WHERE "Address" NOT IN (
                    SELECT min("Address")
                    FROM "DisallowedEmailAddresses"
                    GROUP BY lower("Address")
                )
            """);
            // for some reason, Postgres won't actually execute these separately if we use semicolons, so we have to do separate method calls
            migrationBuilder.Sql
            ("""
                UPDATE "DisallowedEmailAddresses" SET "Address" = lower("Address");
            """);

            // Email Domains
            migrationBuilder.Sql
            ("""
                DELETE FROM "DisallowedEmailDomains"
                WHERE "Domain" NOT IN (
                    SELECT min("Domain")
                    FROM "DisallowedEmailDomains"
                    GROUP BY lower("Domain")
                )
            """);
            migrationBuilder.Sql
            ("""
                UPDATE "DisallowedEmailDomains" SET "Domain" = lower("Domain");
            """);

            // Usernames
            migrationBuilder.Sql
            ("""
                DELETE FROM "DisallowedUsers"
                WHERE "Username" NOT IN (
                    SELECT min("Username")
                    FROM "DisallowedUsers"
                    GROUP BY lower("Username")
                )
            """);
            migrationBuilder.Sql
            ("""
                UPDATE "DisallowedUsers" SET "Username" = lower("Username");
            """);

            // Assets
            migrationBuilder.Sql
            ("""
                DELETE FROM "DisallowedAssets"
                WHERE "AssetHash" NOT IN (
                    SELECT min("AssetHash")
                    FROM "DisallowedAssets"
                    GROUP BY lower("AssetHash")
                )
            """);
            migrationBuilder.Sql
            ("""
                UPDATE "DisallowedAssets" SET "AssetHash" = lower("AssetHash");
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
