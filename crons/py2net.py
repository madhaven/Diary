# this file helps convert diary data files saved using the Filer_3_2 class into sqlite databases
# This is to facilitate the conversion of the project to the .NET.

import os
from os import sep, path
import sys
import sqlite3

# Add the parent directory to the Python path so we can import from diary
sys.path.append(path.dirname(path.dirname(path.abspath(__file__))))
from diary import Filer_3_2, Diary

# fetch filepath
try:
    filelocation = ""
    exec(open(sep.join([path.expanduser('~'), 'diary_config']), 'r').read())
except:
    print("Migration was not able to find an appropriate diary file")
    exit(1)

CREATE_ENTRY_QUERY = """INSERT INTO Entries (Time, Text, Intervals) VALUES (?, ?, ?)"""
CREATE_TABLE_QUERY = """
CREATE TABLE IF NOT EXISTS Entries (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Time TEXT NOT NULL,
    Text TEXT NOT NULL,
    Intervals TEXT NOT NULL
)
"""

diary:Diary = Diary(sep.join(filelocation))
diary.load()

if path.exists("diary.sqlite"):
    os.remove("diary.sqlite")
db = sqlite3.connect("diary.sqlite", )
cur = db.cursor()
cur.execute(CREATE_TABLE_QUERY)

for entry in diary.entries:
    cur.execute(CREATE_ENTRY_QUERY, (
        entry.time.strftime('%Y-%m-%d %H:%M:%S.%f')[:-3],
        entry.text,
        ",".join(map(lambda x: str(int(x*1000)), entry.intervals))
    ))

db.commit()
db.close()
print('Migration complete')
print("place diary.sqlite file in the folder where the Diary.exe tool exists.")