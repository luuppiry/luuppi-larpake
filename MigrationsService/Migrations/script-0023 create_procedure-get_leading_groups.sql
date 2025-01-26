DROP FUNCTION IF EXISTS GetLeadingGroups;
CREATE FUNCTION GetLeadingGroups(
    in_larpake_id BIGINT,
    in_page_size INT,
    in_page_offset INT)
RETURNS TABLE (group_id BIGINT, points INT) AS $$
BEGIN
-- One group can only have one larpake
RETURN QUERY (
    SELECT
        g.id AS group_id,
        SUM(e.points) AS points
    FROM freshman_groups g
        LEFT JOIN freshman_group_members m
            ON g.id = m.group_id
        LEFT JOIN attendances a
            ON a.user_id = m.user_id
        LEFT JOIN larpake_events e
            ON a.larpake_event_id = e.id
        LEFT JOIN larpake_sections s
            ON e.larpake_section_id = s.id
        WHERE a.completion_id IS NOT NULL
            AND s.larpake_id = in_larpake_id
        GROUP BY g.id
            ORDER BY SUM(e.points) DESC
            LIMIT in_page_size
            OFFSET  in_page_offset
);
END;
$$ LANGUAGE PLPGSQL
