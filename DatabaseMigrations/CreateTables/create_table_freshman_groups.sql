CREATE TABLE freshman_groups (
    id BIGSERIAL,
    name VARCHAR(80),
    start_year INT,
    group_number INT,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id)
)