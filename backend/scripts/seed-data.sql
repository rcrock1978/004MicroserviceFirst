-- Optional seed data for local development
-- Run after init-databases.sh and once service migrations have been applied.

-- Connect to the 'tenant' database to seed a default tenant
\c tenant;

INSERT INTO "Tenants" ("Id", "Name", "Slug", "Status", "CreatedAt", "UpdatedAt")
VALUES
  ('11111111-1111-1111-1111-111111111111', 'Default Tenant', 'default', 'Active', NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;

-- Seed default feature flags for the default tenant
INSERT INTO "FeatureFlags" ("Id", "TenantId", "Key", "Enabled", "Description", "CreatedAt", "UpdatedAt")
VALUES
  ('22222222-2222-2222-2222-222222222222', '11111111-1111-1111-1111-111111111111', 'NewCheckoutFlow', true, 'Enable the new checkout experience', NOW(), NOW()),
  ('33333333-3333-3333-3333-333333333333', '11111111-1111-1111-1111-111111111111', 'BetaNotifications', false, 'Enable beta push notifications', NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;

-- Connect to the 'identity' database to seed a mirrored user profile
\c identity;

INSERT INTO "UserProfiles" ("Id", "TenantId", "ExternalId", "Email", "DisplayName", "IsActive", "CreatedAt", "UpdatedAt")
VALUES
  ('44444444-4444-4444-4444-444444444444', '11111111-1111-1111-1111-111111111111', 'auth0|local-dev-001', 'dev@example.com', 'Local Dev User', true, NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;

-- Connect to the 'inventory' database to seed initial stock items
\c inventory;

INSERT INTO "StockItems" ("Id", "TenantId", "ProductId", "QuantityAvailable", "QuantityReserved", "CreatedAt", "UpdatedAt")
VALUES
  ('55555555-5555-5555-5555-555555555555', '11111111-1111-1111-1111-111111111111', 'PROD-001', 100, 0, NOW(), NOW()),
  ('66666666-6666-6666-6666-666666666666', '11111111-1111-1111-1111-111111111111', 'PROD-002', 50, 0, NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;

\echo 'Seed data applied successfully.'
