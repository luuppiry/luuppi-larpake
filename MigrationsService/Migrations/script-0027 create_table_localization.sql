CREATE TABLE larpake_localizations (
    larpake_id BIGINT,
    language_id INT,
    title VARCHAR(80) NOT NULL,
    description TEXT,
    PRIMARY KEY (larpake_id, language_id),
    FOREIGN KEY (larpake_id) REFERENCES larpakkeet(id),
    FOREIGN KEY (language_id) REFERENCES languages(id)
);

    