BEGIN;
-- Use transaction because of multiple tables

-- Larpake localizations
CREATE TABLE larpake_localizations (
    larpake_id BIGINT,
    language_id INT,
    title VARCHAR(80) NOT NULL,
    description TEXT,
    PRIMARY KEY (larpake_id, language_id),
    FOREIGN KEY (larpake_id) REFERENCES larpakkeet(id),
    FOREIGN KEY (language_id) REFERENCES languages(id)
);

-- Larpake section localizations
CREATE TABLE larpake_section_localizations (
    larpake_section_id BIGINT,
    language_id INT,
    title VARCHAR(80) NOT NULL,
    PRIMARY KEY (larpake_section_id, language_id),
    FOREIGN KEY (larpake_section_id) REFERENCES larpake_sections(id),
    FOREIGN KEY (language_id) REFERENCES languages(id)
);

-- Larpake Event localization
CREATE TABLE larpake_event_localizations (
    larpake_event_id BIGINT,
    language_id INT,
    title VARCHAR(80),
    body TEXT,
    PRIMARY KEY (larpake_event_id, language_id),
    FOREIGN KEY (larpake_event_id) REFERENCES larpake_events(id),
    FOREIGN KEY (language_id) REFERENCES languages(id)
);

-- Organization Event localizations
CREATE TABLE organization_event_localizations (
    organization_event_id BIGINT,
    language_id INT,
    title VARCHAR(80),
    body TEXT,
    website_url VARCHAR(150),
    image_url VARCHAR(150),
    PRIMARY KEY (organization_event_id, language_id),
    FOREIGN KEY (organization_event_id) REFERENCES organization_events(id),
    FOREIGN KEY (language_id) REFERENCES languages(id)
);

COMMIT;