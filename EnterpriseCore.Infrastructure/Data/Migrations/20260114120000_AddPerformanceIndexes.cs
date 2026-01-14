using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseCore.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Users: Composite index for filtering active users by tenant
            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_IsActive",
                table: "Users",
                columns: new[] { "TenantId", "IsActive" });

            // Projects: Composite index for filtering projects by status within a tenant
            migrationBuilder.CreateIndex(
                name: "IX_Projects_TenantId_Status",
                table: "Projects",
                columns: new[] { "TenantId", "Status" });

            // Tasks: Composite index for filtering tasks by status within a tenant
            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TenantId_Status",
                table: "Tasks",
                columns: new[] { "TenantId", "Status" });

            // Tasks: Index for ordering tasks by creation date
            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CreatedAt",
                table: "Tasks",
                column: "CreatedAt");

            // ActivityLogs: Composite index for filtering activity logs by date within a tenant
            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_TenantId_CreatedAt",
                table: "ActivityLogs",
                columns: new[] { "TenantId", "CreatedAt" });

            // Tasks: Index for filtering tasks by priority
            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Priority",
                table: "Tasks",
                column: "Priority");

            // Projects: Index for filtering by dates
            migrationBuilder.CreateIndex(
                name: "IX_Projects_StartDate_EndDate",
                table: "Projects",
                columns: new[] { "StartDate", "EndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_IsActive",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Projects_TenantId_Status",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_TenantId_Status",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_CreatedAt",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_ActivityLogs_TenantId_CreatedAt",
                table: "ActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_Priority",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Projects_StartDate_EndDate",
                table: "Projects");
        }
    }
}
