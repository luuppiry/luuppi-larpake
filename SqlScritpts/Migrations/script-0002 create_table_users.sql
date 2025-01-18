CREATE TABLE users (
    id UUID DEFAULT uuid_generate_v7(),
    permissions INT NOT NULL DEFAULT 0,
    start_year INT,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id)
);