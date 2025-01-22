CREATE TABLE attendances (
    user_id UUID,
    larpake_event_id BIGINT,
    completion_id UUID,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    qr_code_key VARCHAR(20) UNIQUE,
    key_invalid_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id, larpake_event_id),
    FOREIGN KEY (user_id) REFERENCES users(id),
    FOREIGN KEY (larpake_event_id) REFERENCES larpake_events(id),
    FOREIGN KEY (completion_id) REFERENCES completions(id)
)