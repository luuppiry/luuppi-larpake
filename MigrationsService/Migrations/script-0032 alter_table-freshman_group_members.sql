-- Add is competing column to track who are tutors and who can sign attendances and who not
-- If this column is true, signing should be denied
ALTER TABLE freshman_group_members
    ADD COLUMN is_competing BOOLEAN NOT NULL DEFAULT TRUE;