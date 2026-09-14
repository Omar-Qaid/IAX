using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IAX.IXApi.Infrastructure.Migrations;

/// <summary>Retire execution without deleting legacy schedule configuration or history.</summary>
[DbContext(typeof(ApplicationDbContext))]
[Migration("20260914200000_RetireWorkflowProcessScheduling")]
public sealed class RetireWorkflowProcessScheduling : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DECLARE @retiredJobs TABLE ([JobId] bigint PRIMARY KEY);
            INSERT INTO @retiredJobs ([JobId])
            SELECT job.[RECID] FROM [BatchJobs] AS job
            WHERE job.[JobKey] = N'WFProcessScheduled'
                OR EXISTS (SELECT 1 FROM [BatchJobTasks] AS task
                    WHERE task.[JobId] = job.[RECID] AND task.[ServiceKey] = N'WFProcessScheduled');
            UPDATE [WFProcessScheduled] SET [Enabled] = 0, [NextRunAt] = NULL;
            UPDATE [BatchJobTasks] SET [IsEnabled] = 0 WHERE [ServiceKey] = N'WFProcessScheduled';
            UPDATE [BatchJobs] SET [IsEnabled] = 0, [Status] = 2, [NextRunAt] = NULL
            WHERE [RECID] IN (SELECT [JobId] FROM @retiredJobs);
            UPDATE execution SET [Status] = 4, [CompletedAt] = SYSUTCDATETIME(),
                [ErrorMessage] = N'Legacy workflow scheduling retired. Configure a registered service in Batch Administration.'
            FROM [BatchJobHistory] AS execution
            INNER JOIN [BatchJobs] AS job ON job.[RECID] = execution.[JobId]
            WHERE job.[RECID] IN (SELECT [JobId] FROM @retiredJobs) AND execution.[Status] = 0;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Never automatically reactivate schedules or cancelled work on rollback.
        // Retained configuration can be reviewed and migrated by an administrator.
    }
}
