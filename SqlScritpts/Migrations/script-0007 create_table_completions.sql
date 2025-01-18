CREATE TABLE completions (
    id UUID,
    signer_id UUID NOT NULL,
    signature_id UUID,
    completed_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    FOREIGN KEY (signer_id) REFERENCES users(id),
    FOREIGN KEY (signature_id) REFERENCES signatures(id)
);