-- Add image url for larpake preview
ALTER TABLE larpake_localizations
    ADD COLUMN image_url VARCHAR(150);


-- Insert larpake
DROP FUNCTION IF EXISTS InsertLarpake;
CREATE FUNCTION InsertLarpake(
    in_title VARCHAR(80),
    in_year INT,
    in_description TEXT,
    in_language_code VARCHAR(10),
    in_image_url VARCHAR(150))
    RETURNS BIGINT AS
$$
DECLARE
    out_larpake_id BIGINT;
BEGIN
    INSERT INTO larpakkeet (year) VALUES (in_year) RETURNING id INTO out_larpake_id;

    INSERT INTO larpake_localizations (larpake_id, language_id, title, description, image_url)
    VALUES (out_larpake_id,
            (SELECT getlanguageid(in_language_code)),
            in_title,
            in_description,
            in_image_url);
    RETURN out_larpake_id;
END;
$$ LANGUAGE plpgsql;