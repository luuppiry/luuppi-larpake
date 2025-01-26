DROP FUNCTION IF EXISTS GetUserTotal;
CREATE FUNCTION GetUserTotal(in_user_id UUID)
RETURNS TABLE(larpake_id BIGINT, total_points INT) AS $$
BEGIN
RETURN QUERY (SELECT s.larpake_id,
               SUM(e.points)::INT as total_points
                    FROM attendances a
                             LEFT JOIN larpake_events e
                                       ON a.larpake_event_id = e.id
                             LEFT JOIN larpake_sections s
                                       ON e.larpake_section_id = s.id
                    WHERE a.user_id = in_user_id
                        AND a.completion_id IS NOT NULL
                    GROUP BY s.larpake_id);
END;
$$ LANGUAGE PLPGSQL
