DROP FUNCTION IF EXISTS GetLeadingUsers;
CREATE FUNCTION GetLeadingUsers(
    in_larpake_id BIGINT,
    in_page_size INT,
    in_page_offset INT)
RETURNS TABLE (user_id UUID, points INT) AS $$
BEGIN
-- One group can only have one larpake
RETURN QUERY (
    SELECT
        a.user_id AS user_id,
        SUM(e.points)::INT AS points
    FROM attendances a
        LEFT JOIN larpake_events e
            ON a.larpake_event_id = e.id
        LEFT JOIN larpake_sections s
            ON e.larpake_section_id = s.id
        WHERE a.completion_id IS NOT NULL
            AND s.larpake_id = in_larpake_id
        GROUP BY a.user_id
            ORDER BY SUM(e.points) DESC
            LIMIT in_page_size
            OFFSET  in_page_offset);
END;
$$ LANGUAGE PLPGSQL
