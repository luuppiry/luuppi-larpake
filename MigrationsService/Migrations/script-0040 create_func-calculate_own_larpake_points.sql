DROP FUNCTION IF EXISTS GetLarpakeUserTotal;
CREATE FUNCTION GetLarpakeUserTotal(in_user_id UUID, in_larpake_id BIGINT)
    RETURNS TABLE
            (
                section_id             BIGINT,
                ordering_weight_number INTEGER,
                total_points           BIGINT,
                earned_points          BIGINT
            )
AS
$$
BEGIN
    RETURN QUERY (SELECT s.id                                                                as section_id,
                         s.ordering_weight_number,
                         SUM(e.points)                                                       as total_points,
                         SUM(CASE WHEN a.completion_id IS NOT NULL THEN e.points ELSE 0 END) as earned_points
                  FROM larpake_sections s
                           LEFT JOIN larpake_events e
                                     ON s.id = e.larpake_section_id
                           LEFT JOIN attendances a
                                     ON e.id = a.larpake_event_id
                                         AND in_user_id = a.user_id
                  WHERE s.larpake_id = in_larpake_id
                  GROUP BY s.id);
END;
$$ LANGUAGE PLPGSQL;
