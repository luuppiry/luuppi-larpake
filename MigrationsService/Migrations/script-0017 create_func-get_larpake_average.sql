DROP FUNCTION IF EXISTS GetLarpakeAverage;
CREATE FUNCTION GetLarpakeAverage(in_larpake_id BIGINT)
RETURNS INTEGER AS $$
BEGIN
-- Get all completed event attendances -> Group by user -> Sum Points -> Get average
RETURN (SELECT CEIL(AVG(user_points))::INT AS average
        FROM (SELECT SUM(e.points) AS user_points
              FROM attendances a
                   LEFT JOIN users u
                     ON a.user_id = u.id
                   LEFT JOIN larpake_events e
                     ON a.larpake_event_id = e.id
                   LEFT JOIN larpake_sections s
                     ON e.larpake_section_id = s.id
              WHERE a.completion_id IS NOT NULL
                    AND s.larpake_id = in_larpake_id
              GROUP BY a.user_id
          ) points);
END;
$$ LANGUAGE PLPGSQL
