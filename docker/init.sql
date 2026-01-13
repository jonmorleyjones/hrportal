-- Initial database setup with seed data for development

-- Create a test tenant
INSERT INTO "Tenants" ("Id", "Slug", "Name", "SubscriptionTier", "CreatedAt", "IsActive", "Settings", "Branding")
VALUES (
    'a0000000-0000-0000-0000-000000000001',
    'acme',
    'Acme Corporation',
    'professional',
    NOW(),
    true,
    '{"enableNotifications": true, "timezone": "America/New_York", "language": "en"}',
    '{"primaryColor": "#3b82f6", "secondaryColor": "#1e40af"}'
);

INSERT INTO "Tenants" ("Id", "Slug", "Name", "SubscriptionTier", "CreatedAt", "IsActive", "Settings", "Branding")
VALUES (
    'b0000000-0000-0000-0000-000000000002',
    'globex',
    'Globex Industries',
    'starter',
    NOW(),
    true,
    '{"enableNotifications": true, "timezone": "UTC", "language": "en"}',
    '{"primaryColor": "#10b981", "secondaryColor": "#059669"}'
);

-- Create admin users for each tenant (password: "password123")
INSERT INTO "Users" ("Id", "TenantId", "Email", "PasswordHash", "Name", "Role", "CreatedAt", "IsActive")
VALUES (
    'c0000000-0000-0000-0000-000000000001',
    'a0000000-0000-0000-0000-000000000001',
    'admin@acme.com',
    '$2a$10$g4JIGUM.L3SWzBajUSkh7ea3F.1Ca2xY/PNfooQg3RBR7BNLVENzi',
    'John Admin',
    'Admin',
    NOW(),
    true
);

INSERT INTO "Users" ("Id", "TenantId", "Email", "PasswordHash", "Name", "Role", "CreatedAt", "IsActive")
VALUES (
    'd0000000-0000-0000-0000-000000000002',
    'b0000000-0000-0000-0000-000000000002',
    'admin@globex.com',
    '$2a$10$g4JIGUM.L3SWzBajUSkh7ea3F.1Ca2xY/PNfooQg3RBR7BNLVENzi',
    'Jane Admin',
    'Admin',
    NOW(),
    true
);

-- Add some member users
INSERT INTO "Users" ("Id", "TenantId", "Email", "PasswordHash", "Name", "Role", "CreatedAt", "IsActive", "InvitedBy", "InvitedAt")
VALUES (
    'e0000000-0000-0000-0000-000000000001',
    'a0000000-0000-0000-0000-000000000001',
    'member@acme.com',
    '$2a$10$g4JIGUM.L3SWzBajUSkh7ea3F.1Ca2xY/PNfooQg3RBR7BNLVENzi',
    'Bob Member',
    'Member',
    NOW(),
    true,
    'c0000000-0000-0000-0000-000000000001',
    NOW()
);

-- Step 1: Add onboarding surveys first (without ActiveVersionId)
INSERT INTO "OnboardingSurveys" ("Id", "TenantId", "Name", "CurrentVersionNumber", "IsActive", "CreatedAt", "UpdatedAt")
VALUES (
    'f0000000-0000-0000-0000-000000000001',
    'a0000000-0000-0000-0000-000000000001',
    'New Starter Form',
    1,
    true,
    NOW(),
    NOW()
);

INSERT INTO "OnboardingSurveys" ("Id", "TenantId", "Name", "CurrentVersionNumber", "IsActive", "CreatedAt", "UpdatedAt")
VALUES (
    'f0000000-0000-0000-0000-000000000002',
    'b0000000-0000-0000-0000-000000000002',
    'New Starter Form',
    1,
    true,
    NOW(),
    NOW()
);

-- Step 2: Add survey versions
INSERT INTO "OnboardingSurveyVersions" ("Id", "SurveyId", "VersionNumber", "SurveyJson", "CreatedAt")
VALUES (
    'f1000000-0000-0000-0000-000000000001',
    'f0000000-0000-0000-0000-000000000001',
    1,
    '{
        "title": "New Starter Form",
        "description": "Submit details for a new employee joining the team.",
        "logoPosition": "right",
        "pages": [
            {
                "name": "personal",
                "title": "Personal Information",
                "description": "Enter the new starter''s basic details.",
                "elements": [
                    {
                        "type": "text",
                        "name": "firstName",
                        "title": "First Name",
                        "isRequired": true,
                        "placeholder": "Enter first name"
                    },
                    {
                        "type": "text",
                        "name": "lastName",
                        "title": "Last Name",
                        "isRequired": true,
                        "placeholder": "Enter last name"
                    },
                    {
                        "type": "text",
                        "name": "email",
                        "title": "Work Email",
                        "isRequired": true,
                        "inputType": "email",
                        "placeholder": "Enter work email address"
                    },
                    {
                        "type": "text",
                        "name": "phone",
                        "title": "Phone Number",
                        "inputType": "tel",
                        "placeholder": "Enter phone number"
                    }
                ]
            },
            {
                "name": "employment",
                "title": "Employment Details",
                "description": "Provide employment information.",
                "elements": [
                    {
                        "type": "text",
                        "name": "jobTitle",
                        "title": "Job Title",
                        "isRequired": true,
                        "placeholder": "Enter job title"
                    },
                    {
                        "type": "dropdown",
                        "name": "department",
                        "title": "Department",
                        "isRequired": true,
                        "choices": [
                            "Engineering",
                            "Product",
                            "Design",
                            "Marketing",
                            "Sales",
                            "Customer Support",
                            "HR",
                            "Finance",
                            "Operations",
                            "Other"
                        ],
                        "showOtherItem": true,
                        "otherText": "Other (please specify)"
                    },
                    {
                        "type": "text",
                        "name": "startDate",
                        "title": "Start Date",
                        "isRequired": true,
                        "inputType": "date"
                    },
                    {
                        "type": "dropdown",
                        "name": "employmentType",
                        "title": "Employment Type",
                        "isRequired": true,
                        "choices": [
                            "Full-time",
                            "Part-time",
                            "Contract",
                            "Intern"
                        ]
                    },
                    {
                        "type": "text",
                        "name": "manager",
                        "title": "Reporting Manager",
                        "placeholder": "Enter manager''s name"
                    }
                ]
            },
            {
                "name": "equipment",
                "title": "Equipment & Access",
                "description": "Specify what equipment and access the new starter will need.",
                "elements": [
                    {
                        "type": "checkbox",
                        "name": "equipment",
                        "title": "Equipment Required",
                        "choices": [
                            "Laptop",
                            "Monitor",
                            "Keyboard & Mouse",
                            "Headset",
                            "Mobile Phone",
                            "Desk Phone"
                        ],
                        "colCount": 2
                    },
                    {
                        "type": "checkbox",
                        "name": "softwareAccess",
                        "title": "Software Access Required",
                        "choices": [
                            "Email & Calendar",
                            "Slack / Teams",
                            "Project Management Tools",
                            "CRM System",
                            "HR System",
                            "Development Tools",
                            "Design Tools",
                            "Finance Systems"
                        ],
                        "colCount": 2
                    },
                    {
                        "type": "comment",
                        "name": "additionalNotes",
                        "title": "Additional Notes",
                        "placeholder": "Any special requirements or notes about this new starter...",
                        "rows": 3
                    }
                ]
            }
        ],
        "showProgressBar": "top",
        "progressBarType": "buttons",
        "completeText": "Submit New Starter",
        "showQuestionNumbers": "off",
        "questionErrorLocation": "bottom",
        "focusFirstQuestionAutomatic": false,
        "widthMode": "static",
        "width": "900px"
    }',
    NOW()
);

INSERT INTO "OnboardingSurveyVersions" ("Id", "SurveyId", "VersionNumber", "SurveyJson", "CreatedAt")
VALUES (
    'f1000000-0000-0000-0000-000000000002',
    'f0000000-0000-0000-0000-000000000002',
    1,
    '{
        "title": "New Starter Form",
        "description": "Submit details for a new team member.",
        "pages": [
            {
                "name": "intro",
                "elements": [
                    {
                        "type": "text",
                        "name": "fullName",
                        "title": "Full Name",
                        "isRequired": true
                    },
                    {
                        "type": "text",
                        "name": "email",
                        "title": "Email Address",
                        "isRequired": true,
                        "inputType": "email"
                    },
                    {
                        "type": "radiogroup",
                        "name": "department",
                        "title": "Which department will they be joining?",
                        "isRequired": true,
                        "choices": ["Engineering", "Sales", "Marketing", "Operations", "HR", "Finance", "Other"]
                    },
                    {
                        "type": "text",
                        "name": "startDate",
                        "title": "Start Date",
                        "isRequired": true,
                        "inputType": "date"
                    }
                ]
            }
        ],
        "showProgressBar": "bottom",
        "completeText": "Submit",
        "showQuestionNumbers": "off"
    }',
    NOW()
);

-- Step 3: Update surveys to set ActiveVersionId
UPDATE "OnboardingSurveys"
SET "ActiveVersionId" = 'f1000000-0000-0000-0000-000000000001'
WHERE "Id" = 'f0000000-0000-0000-0000-000000000001';

UPDATE "OnboardingSurveys"
SET "ActiveVersionId" = 'f1000000-0000-0000-0000-000000000002'
WHERE "Id" = 'f0000000-0000-0000-0000-000000000002';
