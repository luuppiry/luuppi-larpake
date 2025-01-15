CREATE TABLE larpake_events (
    id BIGSERIAL,
    larpake_section_id BIGINT NOT NULL,
    title VARCHAR(80) NOT NULL,
    body TEXT,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    cancelled_at TIMESTAMPTZ,
    PRIMARY KEY (id),
    FOREIGN KEY (larpake_section_id) REFERENCES larpake_sections(id)
);