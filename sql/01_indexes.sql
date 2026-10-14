-- Case-insensitive title search (prefix) uses this function-based index.
CREATE INDEX idx_movies_title_upper ON movies (UPPER(title));

BEGIN
  DBMS_STATS.GATHER_TABLE_STATS(USER, 'MOVIES', cascade => TRUE);
END;
/
