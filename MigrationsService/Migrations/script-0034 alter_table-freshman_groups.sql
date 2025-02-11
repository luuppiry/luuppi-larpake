-- Add new columns to support creating invite keys for groups
ALTER TABLE freshman_groups
    ADD COLUMN invite_key            VARCHAR(10),
    ADD COLUMN invite_key_changed_at TIMESTAMPTZ,
    ADD CONSTRAINT invite_key_null_or_unique UNIQUE (invite_key);

