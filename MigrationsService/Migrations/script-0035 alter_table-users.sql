-- Add new column to store entra id username/email
ALTER TABLE users
    ADD COLUMN entra_username VARCHAR(255),
    ADD CONSTRAINT entra_username_unique_or_null UNIQUE (entra_username);
