DROP FUNCTION IF EXISTS GetGroupTotalByUser;
CREATE FUNCTION GetGroupTotalByUser(
    in_user_id UUID)
RETURNS TABLE (
    larpake_id BIGINT,
    group_id BIGINT,
    total_points INT) AS $$
BEGIN
-- One group can only have one larpake
-- Get groups user is in -> filter by completed -> group by group id -> get points sum
RETURN QUERY (
    SELECT
        s.larpake_id,
        m.group_id AS group_id,
        SUM(e.points)::INT AS points
    FROM attendances a
        LEFT JOIN larpake_events e
            ON a.larpake_event_id = e.id
        LEFT JOIN larpake_sections s
            ON e.larpake_section_id = s.id
        LEFT JOIN freshman_group_members m
            ON a.user_id = m.user_id
    WHERE a.completion_id IS NOT NULL
        AND m.group_id IN (
            SELECT fgm.group_id FROM freshman_group_members fgm
            WHERE a.user_id = in_user_id
        )
    GROUP BY m.group_id, s.larpake_id);
END;
$$ LANGUAGE PLPGSQL
