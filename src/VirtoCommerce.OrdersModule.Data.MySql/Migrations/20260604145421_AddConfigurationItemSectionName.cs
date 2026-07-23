using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.OrdersModule.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurationItemSectionName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                SET @columnExists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'OrderConfigurationItem' AND COLUMN_NAME = 'SectionName' AND TABLE_SCHEMA = DATABASE());
                SET @sql = IF(@columnExists = 0,
                    'ALTER TABLE `OrderConfigurationItem` ADD COLUMN `SectionName` varchar(256) CHARACTER SET utf8mb4 NULL',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                SET @columnExists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'OrderConfigurationItem' AND COLUMN_NAME = 'SectionName' AND TABLE_SCHEMA = DATABASE());
                SET @sql = IF(@columnExists > 0,
                    'ALTER TABLE `OrderConfigurationItem` DROP COLUMN `SectionName`',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);
        }
    }
}
