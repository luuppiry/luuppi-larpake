CREATE TABLE refresh_tokens
(
    user_id        UUID        NOT NULL,
    token          VARCHAR(64) NOT NULL,
    token_family   UUID        NOT NULL,
    invalid_at     TIMESTAMPTZ NOT NULL,
    invalidated_at TIMESTAMPTZ,
    created_at     TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (token),
    FOREIGN KEY (user_id) REFERENCES users (id)
)