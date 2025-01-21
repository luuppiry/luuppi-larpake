CREATE TABLE larpake_sections (
    id BIGSERIAL,
    larpake_id BIGINT NOT NULL,
    title VARCHAR(80) NOT NULL,
    ordering_weight_number INT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    FOREIGN KEY (larpake_id) REFERENCES larpakkeet(id)
)