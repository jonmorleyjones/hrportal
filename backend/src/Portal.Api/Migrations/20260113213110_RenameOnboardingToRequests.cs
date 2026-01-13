using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portal.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameOnboardingToRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if old tables exist and rename them, otherwise create new tables
            // This handles both fresh installs and upgrades from existing onboarding system

            // First, try to rename existing tables if they exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- Check if OnboardingSurveys table exists
                    IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'OnboardingSurveys') THEN
                        -- Rename OnboardingSurveys to RequestTypes
                        ALTER TABLE ""OnboardingSurveys"" RENAME TO ""RequestTypes"";

                        -- Add new columns to RequestTypes
                        ALTER TABLE ""RequestTypes"" ADD COLUMN IF NOT EXISTS ""Description"" VARCHAR(500) NULL;
                        ALTER TABLE ""RequestTypes"" ADD COLUMN IF NOT EXISTS ""Icon"" VARCHAR(50) NOT NULL DEFAULT 'clipboard-list';

                        -- Rename constraints
                        ALTER INDEX IF EXISTS ""PK_OnboardingSurveys"" RENAME TO ""PK_RequestTypes"";
                        ALTER INDEX IF EXISTS ""IX_OnboardingSurveys_ActiveVersionId"" RENAME TO ""IX_RequestTypes_ActiveVersionId"";
                        ALTER INDEX IF EXISTS ""IX_OnboardingSurveys_TenantId"" RENAME TO ""IX_RequestTypes_TenantId"";

                        -- Rename foreign key constraints
                        ALTER TABLE ""RequestTypes"" RENAME CONSTRAINT ""FK_OnboardingSurveys_Tenants_TenantId"" TO ""FK_RequestTypes_Tenants_TenantId"";
                    END IF;

                    -- Check if OnboardingSurveyVersions table exists
                    IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'OnboardingSurveyVersions') THEN
                        -- Rename OnboardingSurveyVersions to RequestTypeVersions
                        ALTER TABLE ""OnboardingSurveyVersions"" RENAME TO ""RequestTypeVersions"";

                        -- Rename column SurveyId to RequestTypeId
                        ALTER TABLE ""RequestTypeVersions"" RENAME COLUMN ""SurveyId"" TO ""RequestTypeId"";

                        -- Rename column SurveyJson to FormJson
                        ALTER TABLE ""RequestTypeVersions"" RENAME COLUMN ""SurveyJson"" TO ""FormJson"";

                        -- Rename constraints
                        ALTER INDEX IF EXISTS ""PK_OnboardingSurveyVersions"" RENAME TO ""PK_RequestTypeVersions"";
                        ALTER INDEX IF EXISTS ""IX_OnboardingSurveyVersions_SurveyId_VersionNumber"" RENAME TO ""IX_RequestTypeVersions_RequestTypeId_VersionNumber"";

                        -- Rename foreign key constraints
                        ALTER TABLE ""RequestTypeVersions"" RENAME CONSTRAINT ""FK_OnboardingSurveyVersions_OnboardingSurveys_SurveyId"" TO ""FK_RequestTypeVersions_RequestTypes_RequestTypeId"";
                    END IF;

                    -- Check if OnboardingResponses table exists
                    IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'OnboardingResponses') THEN
                        -- Rename OnboardingResponses to RequestResponses
                        ALTER TABLE ""OnboardingResponses"" RENAME TO ""RequestResponses"";

                        -- Rename column SurveyVersionId to RequestTypeVersionId
                        ALTER TABLE ""RequestResponses"" RENAME COLUMN ""SurveyVersionId"" TO ""RequestTypeVersionId"";

                        -- Rename constraints
                        ALTER INDEX IF EXISTS ""PK_OnboardingResponses"" RENAME TO ""PK_RequestResponses"";
                        ALTER INDEX IF EXISTS ""IX_OnboardingResponses_SurveyVersionId"" RENAME TO ""IX_RequestResponses_RequestTypeVersionId"";
                        ALTER INDEX IF EXISTS ""IX_OnboardingResponses_UserId"" RENAME TO ""IX_RequestResponses_UserId"";

                        -- Rename foreign key constraints
                        ALTER TABLE ""RequestResponses"" RENAME CONSTRAINT ""FK_OnboardingResponses_OnboardingSurveyVersions_SurveyVersionId"" TO ""FK_RequestResponses_RequestTypeVersions_RequestTypeVersionId"";
                        ALTER TABLE ""RequestResponses"" RENAME CONSTRAINT ""FK_OnboardingResponses_Users_UserId"" TO ""FK_RequestResponses_Users_UserId"";
                    END IF;

                    -- Update ActiveVersion foreign key if it exists
                    IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'RequestTypes') THEN
                        BEGIN
                            ALTER TABLE ""RequestTypes"" RENAME CONSTRAINT ""FK_OnboardingSurveys_OnboardingSurveyVersions_ActiveVersionId"" TO ""FK_RequestTypes_RequestTypeVersions_ActiveVersionId"";
                        EXCEPTION WHEN undefined_object THEN
                            NULL; -- Constraint doesn't exist, ignore
                        END;
                    END IF;
                END $$;
            ");

            // Create tables if they don't exist (fresh install case)
            migrationBuilder.Sql(@"
                -- Create RequestTypes if it doesn't exist
                CREATE TABLE IF NOT EXISTS ""RequestTypes"" (
                    ""Id"" uuid NOT NULL,
                    ""TenantId"" uuid NOT NULL,
                    ""Name"" VARCHAR(200) NOT NULL,
                    ""Description"" VARCHAR(500) NULL,
                    ""Icon"" VARCHAR(50) NOT NULL DEFAULT 'clipboard-list',
                    ""CurrentVersionNumber"" INTEGER NOT NULL,
                    ""ActiveVersionId"" uuid NULL,
                    ""IsActive"" BOOLEAN NOT NULL,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL,
                    ""UpdatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL,
                    CONSTRAINT ""PK_RequestTypes"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_RequestTypes_Tenants_TenantId"" FOREIGN KEY (""TenantId"") REFERENCES ""Tenants"" (""Id"") ON DELETE CASCADE
                );

                -- Create indexes for RequestTypes if they don't exist
                CREATE INDEX IF NOT EXISTS ""IX_RequestTypes_TenantId"" ON ""RequestTypes"" (""TenantId"");
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_RequestTypes_ActiveVersionId"" ON ""RequestTypes"" (""ActiveVersionId"");

                -- Create RequestTypeVersions if it doesn't exist
                CREATE TABLE IF NOT EXISTS ""RequestTypeVersions"" (
                    ""Id"" uuid NOT NULL,
                    ""RequestTypeId"" uuid NOT NULL,
                    ""VersionNumber"" INTEGER NOT NULL,
                    ""FormJson"" TEXT NOT NULL,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL,
                    CONSTRAINT ""PK_RequestTypeVersions"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_RequestTypeVersions_RequestTypes_RequestTypeId"" FOREIGN KEY (""RequestTypeId"") REFERENCES ""RequestTypes"" (""Id"") ON DELETE CASCADE
                );

                -- Create indexes for RequestTypeVersions if they don't exist
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_RequestTypeVersions_RequestTypeId_VersionNumber"" ON ""RequestTypeVersions"" (""RequestTypeId"", ""VersionNumber"");

                -- Create RequestResponses if it doesn't exist
                CREATE TABLE IF NOT EXISTS ""RequestResponses"" (
                    ""Id"" uuid NOT NULL,
                    ""RequestTypeVersionId"" uuid NOT NULL,
                    ""UserId"" uuid NOT NULL,
                    ""ResponseJson"" TEXT NOT NULL,
                    ""IsComplete"" BOOLEAN NOT NULL,
                    ""StartedAt"" TIMESTAMP WITH TIME ZONE NOT NULL,
                    ""CompletedAt"" TIMESTAMP WITH TIME ZONE NULL,
                    CONSTRAINT ""PK_RequestResponses"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_RequestResponses_RequestTypeVersions_RequestTypeVersionId"" FOREIGN KEY (""RequestTypeVersionId"") REFERENCES ""RequestTypeVersions"" (""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_RequestResponses_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE CASCADE
                );

                -- Create indexes for RequestResponses if they don't exist
                CREATE INDEX IF NOT EXISTS ""IX_RequestResponses_RequestTypeVersionId"" ON ""RequestResponses"" (""RequestTypeVersionId"");
                CREATE INDEX IF NOT EXISTS ""IX_RequestResponses_UserId"" ON ""RequestResponses"" (""UserId"");

                -- Add ActiveVersion foreign key to RequestTypes if it doesn't exist
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE constraint_name = 'FK_RequestTypes_RequestTypeVersions_ActiveVersionId'
                    ) THEN
                        ALTER TABLE ""RequestTypes"" ADD CONSTRAINT ""FK_RequestTypes_RequestTypeVersions_ActiveVersionId""
                        FOREIGN KEY (""ActiveVersionId"") REFERENCES ""RequestTypeVersions"" (""Id"") ON DELETE SET NULL;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert table renames back to onboarding names
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- Check if RequestResponses table exists
                    IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'RequestResponses') THEN
                        ALTER TABLE ""RequestResponses"" RENAME TO ""OnboardingResponses"";
                        ALTER TABLE ""OnboardingResponses"" RENAME COLUMN ""RequestTypeVersionId"" TO ""SurveyVersionId"";
                        ALTER INDEX IF EXISTS ""PK_RequestResponses"" RENAME TO ""PK_OnboardingResponses"";
                        ALTER INDEX IF EXISTS ""IX_RequestResponses_RequestTypeVersionId"" RENAME TO ""IX_OnboardingResponses_SurveyVersionId"";
                        ALTER INDEX IF EXISTS ""IX_RequestResponses_UserId"" RENAME TO ""IX_OnboardingResponses_UserId"";
                    END IF;

                    -- Check if RequestTypeVersions table exists
                    IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'RequestTypeVersions') THEN
                        ALTER TABLE ""RequestTypeVersions"" RENAME TO ""OnboardingSurveyVersions"";
                        ALTER TABLE ""OnboardingSurveyVersions"" RENAME COLUMN ""RequestTypeId"" TO ""SurveyId"";
                        ALTER TABLE ""OnboardingSurveyVersions"" RENAME COLUMN ""FormJson"" TO ""SurveyJson"";
                        ALTER INDEX IF EXISTS ""PK_RequestTypeVersions"" RENAME TO ""PK_OnboardingSurveyVersions"";
                        ALTER INDEX IF EXISTS ""IX_RequestTypeVersions_RequestTypeId_VersionNumber"" RENAME TO ""IX_OnboardingSurveyVersions_SurveyId_VersionNumber"";
                    END IF;

                    -- Check if RequestTypes table exists
                    IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'RequestTypes') THEN
                        ALTER TABLE ""RequestTypes"" DROP COLUMN IF EXISTS ""Description"";
                        ALTER TABLE ""RequestTypes"" DROP COLUMN IF EXISTS ""Icon"";
                        ALTER TABLE ""RequestTypes"" RENAME TO ""OnboardingSurveys"";
                        ALTER INDEX IF EXISTS ""PK_RequestTypes"" RENAME TO ""PK_OnboardingSurveys"";
                        ALTER INDEX IF EXISTS ""IX_RequestTypes_ActiveVersionId"" RENAME TO ""IX_OnboardingSurveys_ActiveVersionId"";
                        ALTER INDEX IF EXISTS ""IX_RequestTypes_TenantId"" RENAME TO ""IX_OnboardingSurveys_TenantId"";
                    END IF;
                END $$;
            ");
        }
    }
}
