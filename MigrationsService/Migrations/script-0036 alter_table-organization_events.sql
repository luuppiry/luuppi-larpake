
-- Create new column
ALTER TABLE organization_event_localizations
    ADD COLUMN location VARCHAR(100);

UPDATE organization_event_localizations AS l
SET
    location = e.location
FROM organization_events e
    WHERE
        l.organization_event_id = e.id;

ALTER  TABLE organization_events
    DROP COLUMN location;

