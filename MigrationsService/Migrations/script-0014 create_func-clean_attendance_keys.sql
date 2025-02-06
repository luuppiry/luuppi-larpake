-- Remove all keys that are invalidated over 5 days ago
-- Remove all not completed entries with no keys
DROP FUNCTION IF EXISTS CleanAttendanceKeys;
CREATE FUNCTION CleanAttendanceKeys()
RETURNS INTEGER AS $$
DECLARE
    affected_update INTEGER;
    affected_delete INTEGER;
    affected_rows INTEGER;
BEGIN
    -- Delete invalidated keys
    UPDATE attendances
        SET qr_code_key = NULL
        WHERE key_invalid_at + interval  '5 day' < NOW();

    GET DIAGNOSTICS  affected_update = ROW_COUNT;

    -- Delete not needed rows
    DELETE FROM attendances
        WHERE completion_id IS NULL
              AND  qr_code_key IS NULL;

    GET DIAGNOSTICS affected_delete = ROW_COUNT;

    -- Return how many rows affected
    affected_rows := affected_update + affected_delete;
    RETURN affected_rows;
END;
$$ LANGUAGE PLPGSQL;



