-- Run in transaction to

-- Insert larpake
DROP FUNCTION IF EXISTS InsertLarpake;
CREATE FUNCTION InsertLarpake(
    in_title VARCHAR(80),
    in_year INT,
    in_description TEXT,
    in_language_code VARCHAR(10))
    RETURNS BIGINT AS
$$
DECLARE
    out_larpake_id BIGINT;
BEGIN
    INSERT INTO larpakkeet (year) VALUES (in_year) RETURNING id INTO out_larpake_id;

    INSERT INTO larpake_localizations (larpake_id, language_id, title, description)
    VALUES (out_larpake_id,
            (SELECT getlanguageid(in_language_code)),
            in_title,
            in_description);
    RETURN out_larpake_id;
END;
$$ LANGUAGE plpgsql;


-- Insert larpake section
DROP FUNCTION IF EXISTS InsertLarpakeSection;
CREATE FUNCTION InsertLarpakeSection(
    in_larpake_id BIGINT,
    in_title VARCHAR(80),
    in_ordering_weight_number INT,
    in_language_code VARCHAR(10))
    RETURNS BIGINT AS
$$
DECLARE
    out_section_id BIGINT;
BEGIN
    INSERT INTO larpake_sections (larpake_id,
                                  ordering_weight_number)
    VALUES (in_larpake_id, in_ordering_weight_number)
    RETURNING id INTO out_section_id;

    INSERT INTO larpake_section_localizations (larpake_section_id,
                                               language_id,
                                               title)
    VALUES (out_section_id,
            (SELECT getlanguageid(in_language_code)),
            in_title);
    RETURN out_section_id;
END;
$$ LANGUAGE plpgsql;


-- Insert larpake event
DROP FUNCTION IF EXISTS InsertLarpakeEvent;
CREATE FUNCTION InsertLarpakeEvent(
    in_larpake_section_id BIGINT,
    in_points INT,
    in_ordering_weight_number INT,
    in_title VARCHAR(80),
    in_body TEXT,
    in_language_code VARCHAR(10))
    RETURNS BIGINT AS
$$
DECLARE
    out_event_id BIGINT;
BEGIN
    INSERT INTO larpake_events (larpake_section_id,
                                points,
                                ordering_weight_number)
    VALUES (in_larpake_section_id,
            in_points,
            in_ordering_weight_number)
    RETURNING id INTO out_event_id;

    INSERT INTO larpake_event_localizations(larpake_event_id,
                                            language_id,
                                            title,
                                            body)
    VALUES (out_event_id,
            (SELECT getlanguageid(in_language_code)),
            in_title,
            in_body);
    RETURN out_event_id;
END;
$$ LANGUAGE plpgsql;


-- Insert organization event section
DROP FUNCTION IF EXISTS InsertOrganizationEvent;
CREATE FUNCTION InsertOrganizationEvent(
    in_title TEXT,
    in_body TEXT,
    in_starts_at TIMESTAMPTZ,
    in_ends_at TIMESTAMPTZ,
    in_location TEXT,
    in_created_by UUID,
    in_website_url TEXT,
    in_image_url TEXT,
    in_language_code VARCHAR(10))
    RETURNS BIGINT AS
$$
DECLARE
    out_event_id BIGINT;
BEGIN

    INSERT INTO organization_events (starts_at,
                                     ends_at,
                                     location,
                                     created_by,
                                     updated_by)
    VALUES (in_starts_at,
            in_ends_at,
            in_location,
            in_created_by,
            in_created_by)
    RETURNING id INTO out_event_id;

    INSERT INTO organization_event_localizations (organization_event_id,
                                                  language_id,
                                                  title,
                                                  body,
                                                  website_url,
                                                  image_url)
    VALUES (out_event_id,
            (SELECT getlanguageid(in_language_code)),
            in_title,
            in_body,
            in_website_url,
            in_image_url);

    RETURN out_event_id;
END;
$$ LANGUAGE plpgsql;



