-- Insert organization event section
DROP FUNCTION IF EXISTS InsertOrganizationEvent;
CREATE FUNCTION InsertOrganizationEvent(
    in_title TEXT,
    in_body TEXT,
    in_starts_at TIMESTAMPTZ,
    in_ends_at TIMESTAMPTZ,
    in_external_id VARCHAR(100),
    in_created_by UUID,
    in_location TEXT,
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
                                     created_by,
                                     updated_by,
                                     external_id)
    VALUES (in_starts_at,
            in_ends_at,
            in_created_by,
            in_created_by,
            in_external_id)
    RETURNING id INTO out_event_id;

    INSERT INTO organization_event_localizations (organization_event_id,
                                                  language_id,
                                                  title,
                                                  body,
                                                  location,
                                                  website_url,
                                                  image_url)
    VALUES (out_event_id,
            (SELECT getlanguageid(in_language_code)),
            in_title,
            in_body,
            in_location,
            in_website_url,
            in_image_url);

    RETURN out_event_id;
END;
$$ LANGUAGE plpgsql;