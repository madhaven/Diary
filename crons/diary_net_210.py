# This Script helps convert dotnet data file back into a format that FILER_210 can process

import os
import sqlite3
from diary import Filer_2_10, Entry
from datetime import datetime

def get_diary_path() -> str:
    roaming_path = os.environ.get('APPDATA')
    if not roaming_path:
        return None

    full_path = os.path.join(roaming_path, "Diary")
    file_path = os.path.join(full_path, "diary.sqlite")
    return file_path    

def fetch_all_diary_entries(db_path):

    # Check if the database file exists before attempting to connect
    if not os.path.exists(db_path):
        print(f"Error: Database file not found at path: {db_path}")
        return None
        
    connection = None
    try:
        connection = sqlite3.connect(db_path)
        cursor = connection.cursor()
        cursor.execute(f"SELECT * FROM Entries")
        all_records = cursor.fetchall()
        return all_records

    except sqlite3.Error as e:
        print(f"An SQLite error occurred: {e}")
        return []

    finally:
        if connection:
            connection.close()

def convert_entries(entries: list) -> list[Entry]:
    converted = []
    for row in entries:
        id, date, text, times = row

        date_time = datetime.strptime(date[:-2], "%Y-%m-%d %H:%M:%S.%f")
        intervals = list(map(lambda x: int(x) / 1000, str(times).split(',')))
        entry = Entry(text, date_time, intervals, False)
        converted.append(entry)
    return converted

def print_entries(entries: list[Entry]):
    if not entries: return
    for row in entries:
        id, date, text, times = row
        print(date, text, end='') 

if __name__ == '__main__':
    db_path = get_diary_path()
    if not db_path:
        print("dbpath not found")
        quit()
    else:
        print(f'reading {db_path}')

    entries = fetch_all_diary_entries(db_path)
    entries2 = convert_entries(entries)

    diary_path = os.path.join('C:', 'Users', 'Jithin', 'Desktop', 'diary')
    filer = Filer_2_10(diary_path)
    filer.write(*entries2)
    print("Write complete")
