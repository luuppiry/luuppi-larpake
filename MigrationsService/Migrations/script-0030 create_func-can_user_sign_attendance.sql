DROP FUNCTION IF EXISTS CanUserSignAttendance;
CREATE FUNCTION CanUserSignAttendance(
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
                                   LEFT JOIN freshman_groups g
                                             ON s.larpake_id = g.larpake_id
                                   LEFT JOIN freshman_group_members m
                                             ON g.id = m.group_id
                          WHERE e.id = in_larpake_event_id
                            AND m.user_id = in_user_id
                            AND m.is_competing = FALSE));
END;
$$ LANGUAGE PLPGSQL
