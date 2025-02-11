DROP FUNCTION IF EXISTS GetLarpakeTotalByUser;
CREATE FUNCTION GetLarpakeTotalByUser(in_user_id UUID)
RETURNS TABLE(larpake_id BIGINT, total_points INT) AS $$
BEGIN
-- Get all completed attendances -> Group by user and larpake -> Sum By User -> Group By Larpake -> Sum to total
RETURN QUERY (
    SELECT
        out_larpake_id AS larpake_id,
        CEIL(SUM(total_user_points))::NUMERIC::INT AS average
    FROM (
        SELECT
            s.larpake_id AS out_larpake_id,
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
    GROUP BY out_larpake_id);
END;
$$ LANGUAGE PLPGSQL
