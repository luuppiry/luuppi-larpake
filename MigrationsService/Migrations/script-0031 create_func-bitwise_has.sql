DROP FUNCTION IF EXISTS BitwiseHas;
CREATE FUNCTION BitwiseHas(in_value INT, in_reference INT) RETURNS BOOL AS
$$
BEGIN
    RETURN (SELECT in_reference & in_value = in_value);
END;
$$ LANGUAGE PLPGSQL
