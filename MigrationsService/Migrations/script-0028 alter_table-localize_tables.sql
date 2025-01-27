-- If anything fails, transaction fails
BEGIN;

-- Move larpake text values to localization table
INSERT INTO larpake_localizations
SELECT id,
       (SELECT getlanguageid('fi')),
       title,
       description
FROM larpakkeet;

-- Move section text values to localization table
INSERT INTO larpake_section_localizations
SELECT id,
       (SELECT getlanguageid('fi')),
       title
FROM larpake_sections;

-- Move larpake events text values to localization table
INSERT INTO larpake_event_localizations
SELECT id,
       (SELECT getlanguageid('fi')),
       title,
       body
FROM larpake_events;

-- Move organization events text values to localization table
INSERT INTO organization_event_localizations
SELECT id,
       (SELECT getlanguageid('fi')),
       title,
       body,
       website_url,
       image_url
FROM organization_events;

-- Alter tables to remove not needed columns
ALTER TABLE larpakkeet
    DROP COLUMN title,
    DROP COLUMN description;

ALTER TABLE larpake_sections
    DROP COLUMN title;

ALTER TABLE larpake_events
    DROP COLUMN title,
    DROP COLUMN body;

ALTER TABLE organization_events
    DROP COLUMN title,
    DROP COLUMN body,
    DROP COLUMN website_url,
    DROP COLUMN image_url;

COMMIT;