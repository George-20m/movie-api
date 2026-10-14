import os
import oracledb

with oracledb.connect(
    user="movies_app",
    password=os.environ["DB_PASSWORD"],
    dsn="127.0.0.1:1521/FREEPDB1",
) as conn:
    cur = conn.cursor()
    cur.execute("SELECT COUNT(*) FROM movies")
    count = cur.fetchone()[0]
    print("rows in movies:", count)