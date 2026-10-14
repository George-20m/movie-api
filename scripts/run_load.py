import os
import sys
import time

import oracledb

from load_movies import clean_rows

BATCH_SIZE = 5000

INSERT_SQL = """
INSERT INTO movies (id, title, overview, original_language, popularity,
                    release_date, budget, revenue, runtime, status, tagline,
                    vote_average, vote_count, poster_path, backdrop_path)
VALUES (:1, :2, :3, :4, :5, :6, :7, :8, :9, :10, :11, :12, :13, :14, :15)
"""

NUM = oracledb.DB_TYPE_NUMBER
DATE = oracledb.DB_TYPE_DATE


def main(csv_path):
    stats = {"read": 0, "kept": 0, "empty_title": 0, "duplicates": 0}
    started = time.time()
    loaded = 0
    with oracledb.connect(
        user="movies_app",
        password=os.environ["DB_PASSWORD"],
        dsn="127.0.0.1:1521/FREEPDB1",
    ) as conn:
        cur = conn.cursor()
        cur.execute("SELECT COUNT(*) FROM movies")
        if cur.fetchone()[0] != 0:
            sys.exit("movies is not empty. Run TRUNCATE TABLE movies first.")
        cur.setinputsizes(NUM, 400, 2000, 10, NUM, DATE, NUM, NUM, NUM,
                          50, 300, NUM, NUM, 50, 50)
        batch = []
        for row in clean_rows(csv_path, stats):
            batch.append(row)
            if len(batch) == BATCH_SIZE:
                cur.executemany(INSERT_SQL, batch)
                conn.commit()
                loaded += len(batch)
                batch = []
                if loaded % 50000 == 0:
                    print("loaded", loaded)
        if batch:
            cur.executemany(INSERT_SQL, batch)
            conn.commit()
            loaded += len(batch)
    print(stats)
    print("rows loaded:", loaded, "| seconds:", round(time.time() - started, 1))


if __name__ == "__main__":
    main(sys.argv[1])
