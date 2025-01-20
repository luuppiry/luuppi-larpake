CREATE TABLE Signatures (
    id UUID,
    user_id UUID NOT NULL,
    path_data_json JSON NOT NULL,
    height INT NOT NULL,
    width INT NOT NULL,
    line_width INT NOT NULL DEFAULT 2,
    stroke_style VARCHAR(30),
    line_cap VARCHAR(30),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    FOREIGN KEY (user_id) REFERENCES users(id)
)