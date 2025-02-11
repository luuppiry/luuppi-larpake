DROP FUNCTION IF EXISTS GetUserLarpakkeet;
CREATE FUNCTION GetUserLarpakkeet(in_user_id UUID)
RETURNS TABLE (larpake_id BIGINT) AS $$
BEGIN
-- Gets all larpake ids that user is currently attending as a group member
-- Includes larpakkeet where is a tutor
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
