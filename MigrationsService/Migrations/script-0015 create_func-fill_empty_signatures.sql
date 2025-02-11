-- Find all completed attendances that do not have signature
-- Add signature if one found
-- Randomizer efficiency should not be problem,
-- because user should have 5 signature max
DROP FUNCTION IF EXISTS FillSignatureKeys;
CREATE FUNCTION FillSignatureKeys()
RETURNS INTEGER AS $$
DECLARE
    affected_rows INTEGER;
BEGIN
    -- Run update
    UPDATE completions c
        SET signature_id = (
            SELECT id FROM signatures s
              WHERE s.user_id = c.signer_id
              ORDER BY RANDOM() LIMIT 1
            )
        WHERE signature_id IS NULL;

    -- Return how many rows affected
    GET DIAGNOSTICS affected_rows = ROW_COUNT;
    RETURN affected_rows;
END;
$$ LANGUAGE PLPGSQL;



