DROP FUNCTION IF EXISTS GetLarpakeAverageByUser;
CREATE FUNCTION GetLarpakeAverageByUser(in_user_id UUID)
RETURNS TABLE(larpake_id BIGINT, average_points INT) AS $$
BEGIN
-- Get all completed attendances -> Group by user and larpake -> Sum By User -> Group By Larpake -> Get average
RETURN QUERY (
    SELECT
        user_points.larpake_id,
        CEIL(AVG(user_points.total_user_points))::NUMERIC::INT AS average_points
    FROM (
        SELECT
            s.larpake_id,
            SUM(points) AS total_user_points
        FROM larpake_events e
            LEFT JOIN attendances a
                ON e.id = a.larpake_event_id
            LEFT JOIN larpake_sections s
                ON e.larpake_section_id = s.id
        WHERE a.completion_id IS NOT NULL
            AND s.larpake_id IN (SELECT  getuserlarpakkeet(in_user_id))
        GROUP BY s.larpake_id, a.user_id
        ) AS user_points
    GROUP BY user_points.larpake_id);
END;
$$ LANGUAGE PLPGSQL
