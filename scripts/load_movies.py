import csv
import sys
from datetime import date

csv.field_size_limit(sys.maxsize)


def to_number(text):
    return float(text) if text != "" else None


def to_date(text):
    return date.fromisoformat(text) if text != "" else None


def clean_rows(path, stats):
    seen = set()
    with open(path, encoding="utf-8", newline="") as f:
        for r in csv.DictReader(f):
            stats["read"] += 1
            if not r["title"].strip():
                stats["empty_title"] += 1
                continue
            movie_id = int(r["id"])
            if movie_id in seen:
                stats["duplicates"] += 1
                continue
            seen.add(movie_id)
            stats["kept"] += 1
            yield (
                movie_id,
                r["title"],
                r["overview"],
                r["original_language"],
                to_number(r["popularity"]),
                to_date(r["release_date"]),
                to_number(r["budget"]),
                to_number(r["revenue"]),
                to_number(r["runtime"]),
                r["status"],
                r["tagline"],
                to_number(r["vote_average"]),
                to_number(r["vote_count"]),
                r["poster_path"],
                r["backdrop_path"],
            )


if __name__ == "__main__":
    stats = {"read": 0, "kept": 0, "empty_title": 0, "duplicates": 0}
    first = None
    for row in clean_rows(sys.argv[1], stats):
        if first is None:
            first = row
    print(stats)
    print(first)