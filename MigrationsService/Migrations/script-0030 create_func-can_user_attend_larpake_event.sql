DROP FUNCTION IF EXISTS CanUserAttendLarpakeEvent;
CREATE FUNCTION CanUserAttendLarpakeEvent(
    in_user_id UUID,
    in_larpake_event_id BIGINT
)
    RETURNS BOOLEAN AS
$$
BEGIN
    RETURN (SELECT EXISTS(SELECT 1
                          FROM larpake_events e
                                   LEFT JOIN larpake_sections s
                                             ON e.larpake_section_id = s.id
                          WHERE s.larpake_id = ANY (SELECT getuserlarpakkeet(in_user_id))
                            AND e.id = in_larpake_event_id));

END;
$$ LANGUAGE PLPGSQL
