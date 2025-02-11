CREATE TABLE event_map (
    larpake_event_id BIGINT,
    organization_event_id BIGINT,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (larpake_event_id, organization_event_id),
    FOREIGN KEY (larpake_event_id) REFERENCES larpake_events(id),
    FOREIGN KEY (organization_event_id) REFERENCES organization_events(id)
);