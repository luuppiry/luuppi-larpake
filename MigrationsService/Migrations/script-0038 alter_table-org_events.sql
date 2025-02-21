ALTER TABLE organization_events
    ADD COLUMN external_id VARCHAR(100),
    ADD CONSTRAINT external_id_null_or_unique UNIQUE (external_id);