DROP FUNCTION GetUserTotalPoints;
CREATE FUNCTION GetUserTotalPoints(in_user_id UUID)
RETURNS TABLE(larpake_id BIGINT, points INT) AS $$
BEGIN
-- Get all completed attendances -> Group by user and larpake -> Sum By User -> Group By Larpake -> Get average
RETURN QUERY (SELECT s.larpake_id,
                           SUM(e.points) as points
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
