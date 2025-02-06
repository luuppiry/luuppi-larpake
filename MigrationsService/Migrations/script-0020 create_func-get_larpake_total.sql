DROP FUNCTION IF EXISTS GetLarpakeTotal;
CREATE FUNCTION GetLarpakeTotal(in_larpake_id BIGINT)
RETURNS INT AS $$
BEGIN
RETURN (
     SELECT SUM(e.points)::INT
           FROM attendances a
                    LEFT JOIN users u
                              ON a.user_id = u.id
                    LEFT JOIN larpake_events e
                              ON a.larpake_event_id = e.id
                    LEFT JOIN larpake_sections s
                              ON e.larpake_section_id = s.id
           WHERE a.completion_id IS NOT NULL
             AND s.larpake_id = in_larpake_id
);
END;
$$ LANGUAGE plpgsql;
