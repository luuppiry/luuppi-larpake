DROP FUNCTION GetUserLarpakkeet;
CREATE FUNCTION GetUserLarpakkeet(in_user_id UUID)
RETURNS TABLE (larpake_id BIGINT) AS $$
BEGIN
-- Get all completed event attendances -> Group by user -> Sum Points -> Get average
RETURN QUERY (
    SELECT g.larpake_id FROM larpakkeet l
        LEFT JOIN freshman_groups g
            ON l.id = g.larpake_id
        LEFT JOIN freshman_group_members m
            ON g.id = m.group_id
    WHERE m.user_id = in_user_id
);
END;
$$ LANGUAGE plpgsql;
