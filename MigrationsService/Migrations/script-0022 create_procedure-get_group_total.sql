DROP FUNCTION IF EXISTS GetGroupTotal;
CREATE FUNCTION GetGroupTotal(in_group_id BIGINT)
RETURNS INT AS $$
BEGIN
-- One group can only have one larpake
RETURN (SELECT SUM(e.points)
        FROM freshman_groups g
            LEFT JOIN freshman_group_members m
                ON g.id = m.group_id
            LEFT JOIN attendances a
                ON m.user_id = a.user_id
            LEFT JOIN larpake_events e
                ON a.larpake_event_id = e.id
            WHERE a.completion_id IS NOT NULL
                AND g.id = in_group_id
        );
END;
$$ LANGUAGE PLPGSQL
