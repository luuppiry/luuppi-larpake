CREATE TABLE freshman_group_members (
    group_id BIGINT,
    user_id UUID,
    is_hidden BOOLEAN NOT NULL DEFAULT FALSE,
    PRIMARY KEY (group_id, user_id),
    FOREIGN KEY (group_id) REFERENCES freshman_groups(id),
    FOREIGN KEY (user_id) REFERENCES users(id)
);