-- Create static language table

CREATE TABLE languages (
    id SERIAL,
    code VARCHAR(10),
    language VARCHAR(20),
    PRIMARY KEY (id),
    UNIQUE (code)
);

INSERT INTO languages (code, language) VALUES
     ('fi', 'finnish'),
     ('en', 'english');


DROP FUNCTION IF EXISTS GetLanguageId;
CREATE FUNCTION GetLanguageId(in_lang_code VARCHAR(10))
RETURNS INT AS $$
BEGIN
RETURN (SELECT id FROM languages WHERE code = in_lang_code LIMIT 1);
END;
$$ LANGUAGE PLPGSQL;

DROP FUNCTION IF EXISTS GetLanguageCode;
CREATE FUNCTION GetLanguageCode(in_lang_id INT)
RETURNS VARCHAR(10) AS $$
BEGIN
RETURN (SELECT code FROM languages WHERE id = in_lang_id LIMIT 1);
END;
$$ LANGUAGE PLPGSQL;

