CREATE TABLE larpake_sections (
    id BIGSERIAL,
    larpake_id BIGINT NOT NULL,
    title VARCHAR(80) NOT NULL,
    section_sequence_number INT,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    FOREIGN KEY (larpake_id) REFERENCES larpakkeet(id)
)