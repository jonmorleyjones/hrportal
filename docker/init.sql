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

-- Create admin users for each tenant (password: "  ")
-- BCrypt hash for "password123"
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
