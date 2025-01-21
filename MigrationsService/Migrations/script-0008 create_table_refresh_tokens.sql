CREATE TABLE refresh_tokens (
    user_id UUID,
    token VARCHAR(64),
    token_family UUID NOT NULL,
    invalid_at TIMESTAMPTZ NOT NULL,
    invalidated_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id, token),
    FOREIGN KEY (user_id) REFERENCES users(id)
)