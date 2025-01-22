-- Empty for now
-- Maybe add uuid_v7 extension here like
-- CREATE EXTENSION IF NOT EXISTS "pg_uuidv7";

CREATE TYPE POINT2D AS (
    X INT,
    Y INT
);