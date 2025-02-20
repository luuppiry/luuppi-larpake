ALTER TABLE organization_events
    ADD COLUMN externalId VARCHAR(100),
    ADD CONSTRAINT external_id_null_or_unique UNIQUE (externalId);