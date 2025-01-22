CREATE TABLE freshman_groups (
    id BIGSERIAL,
    larpake_id BIGINT,
    name VARCHAR(80),
    group_number INT,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (larpake_id) REFERENCES larpakkeet(id),
    PRIMARY KEY (id)
)