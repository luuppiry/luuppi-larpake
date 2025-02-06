
-- Add new columns to support entra id auth
ALTER TABLE users
    ADD COLUMN entra_user_id UUID UNIQUE,
    ADD COLUMN username VARCHAR(100) UNIQUE;