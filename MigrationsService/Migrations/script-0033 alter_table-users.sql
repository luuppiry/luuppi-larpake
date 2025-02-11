-- Add new column to support entra id auth
ALTER TABLE users
    ADD COLUMN entra_id UUID,
    ADD CONSTRAINT entra_id_unique_or_null UNIQUE (entra_id);

