DROP TABLE  languages CASCADE;
CREATE TABLE languages (
    id VARCHAR(10),
    language VARCHAR(20),
    PRIMARY KEY (id)
);

INSERT INTO languages (id, language) VALUES
     ('fi', 'finnish'),
     ('en', 'english');
