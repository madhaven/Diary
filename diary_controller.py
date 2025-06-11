import calendar
from datetime import datetime, timedelta
from msvcrt import getch, kbhit
from os import system
import re
from time import sleep

from DiaryConfig import *
from diary import Diary
from DiaryExceptions import BadFileHeader, EmergencyStop
from Entry import Entry


class DiaryController():
    '''provide access to Diary'''

    def __init__(self, filename:str, stopWord:str=STOPWORD, typespeed:float=TYPESPEED):
        self.stopWord:str = stopWord
        self.typespeed:int = typespeed
        try:
            self.diary:Diary = Diary(filename)
        except BadFileHeader as e:
            raise NotImplementedError
        self.version:str = VERSION
    
    def _showVersion(self, args=None):
        '''Shows Controller Version'''
        print("Diary.py v" + self.version + "\ngithub.com/madhaven/Diary")

    def _showInfo(self):
        '''Shows Manual'''
        self._showVersion()
        print(
            '\nUsage','-----',
            'diary log|entry - to add to your diary',
            'diary version - to check which version your diary is running',
            'diary read - to access older entries or logs that you have made',
            'diary search|find - to search for keywords',
            'diary searchall|findall - to search for entries containing all the keywords',
            'diary backup [filename]',
            'diary export - to export your entries to portable formats | NOT AVAILABLE',
            sep='\n'
        )

    def main(self, *args):
        '''Main entry point into Diary. parses cli args to select menu'''
        try:
            if not args:
                self._showInfo()
            elif args[0] in ['log', 'entry']:
                self.log()
            elif args[0] in ['read', 'show']:
                self.read(*args[1:])
            elif args[0] in ['search', 'find']:
                self.search(*args[1:])
            elif args[0] in ['searchall', 'findall', 'search all', 'find all']:
                self.search(*args[1:], strictMode=True)
            elif args[0] in ['export as', 'export to', 'export']:
                self.diary.export(*args[1:])
            elif args[0] in ['version', '--version']:
                self._showVersion()
            elif args[0] in ['backup']:
                self.backup(*args[1:])
            else:
                self._showInfo()
        except FileNotFoundError as e:
            print('\nDiary file missing "%s"?'%self.diary.filer.fileName)

    def log(self):
        '''
        To record diary entries\n
        Initiates a loop of Entry recordings\n
        Loop ends when the stop word is found in an Entry\n
        '''
        try:
            while True:
                entry = self._record()
                self.diary.add(entry)
                if self.stopWord in str(entry).lower():
                    break
        except EmergencyStop as e:
            self.diary.add(e.entry)
            system('cls')
        except Exception as e:
            print('your last entry was broken: %s'%entry)
            raise e
    
    def _record(self) -> Entry:
        '''method to record an entry from the cli interface. A single entry ends when the return key is pressed. The diary log ends if the stopword is found in the entry.'''
        try:
            # record current time
            entry = Entry(time=datetime.now())
            t1 = entry.time.timestamp()

            while True:
                # get pressed character
                char = str(getch())[2:-1]

                # format time
                t2 = datetime.now().timestamp()
                if (t2-t1) > 60:
                    t2, t1 = (t2-t1) - int(t2-t1) + 5, t2
                else:
                    t2, t1 = t2 - t1, t2
                t2 = round(t2, 4)

                # return key
                if char == '\\r':
                    
                    # do nothing if text is empty
                    if not entry:
                        continue

                    # add a new line character to the entry and format the entry object
                    entry.addChar('\n', t2)
                    print()
                    return entry
                
                # ctrl+c
                elif char == '\\x03':
                    if entry:
                        entry.addChar('\n', t2)
                    raise EmergencyStop(entry)
                
                # escape char
                elif char == '\\\\':
                    print('\\', end='', flush=True)
                    char = '\\'
                
                # tabspace
                elif char == '\\t':
                    print('\t', end='', flush=True)
                    char = '\t'
                
                # backspace?
                elif char == '\\x08':
                    print('\b \b', end='', flush=True)
                    char = '\b'
                
                # stray char
                elif len(char) > 1:
                    char = str(getch())[2:-1]
                    continue
                
                # normal char
                else:
                    print(char, end='', flush=True)
                
                # add char to entry list
                entry.addChar(char, t2)
        
        except Exception as e:
            print(e)
            raise e
    
    def read(self, *args):
        '''displays the diary entries of the queried day'''
        if not args:
            print( 
                'Usage',
                '-----',
                'diary read all',
                'diary read today',
                'diary read yesterday',
                'diary read [YYYY] [Mon] [DD]',
                sep='\n'
            )
            return
        
        if args[0]=='all':
            year, month, day, getLatest = None, None, None, False
        elif args[0] in ['yesterday', 'today']:
            args = datetime.now()-timedelta(days=1) if args[0]=='yesterday' else datetime.now()
            year, month, day, getLatest = args.year, args.month, args.day, False
        elif args[0] in ['latest', 'last']:
            year, month, day, getLatest = None, None, None, True
        else:
            try:
                year = list(filter(re.compile(r'^\d{4}$').match, args))
                year = int(year[0]) if year else None
                monthname = list(filter(re.compile(r'^[a-zA-Z]{3}.*$').match, args))
                monthname = monthname[0] if monthname else None
                month = {
                    month[:3].lower(): index
                    for index, month in enumerate(calendar.month_abbr)
                    if month
                }[monthname[:3]] if monthname else None
                day = list(filter(re.compile(r'^\d{1,2}$').match, args))
                day = int(day[0]) if day else None
                getLatest = False
            except:
                print("That date doesn't look right")
                if TESTING: print_exc()
                return
        
        entries = self.diary.filter(year, month, day, getLatest)
        count = len(entries)
        print('%s %s found'%(count, 'entry' if count==1 else 'entries'))
        try:
            for entry in entries:
                self.printEntry(entry)
        except KeyboardInterrupt:
            print("\nDiary closed")
            return
    
    def search(self, *args, strictMode:bool=False):
        '''Search/Find keywords'''
        if not args:
            print( 
                'Usage',
                '-----',
                'diary search [search_text [search_text2 [...]]]',
                'diary searchall [search_text [search_text2 [...]]]',
                'diary searchall matches all strings together',
                'PS : find and findall also works the same',
                sep = '\n'
            )
            return

        results = self.diary.search(*args, strictMode=strictMode)
        for entry in results:
            print(entry.time.strftime('%Y %b %d %H:%M:%S %a'), '|', str(entry), end='')
        count = len(results)
        print("%s %s found"%(count, "entries" if count!=1 else "entry"))
    
    def printEntry(self, entry:Entry, speed:int=None):
        '''
        prints the entry\n
        Contains sleep() to imitate the user's type speed\n
        self.printdate decides whether or not to print the timestamp\n
        '''
        speed = self.typespeed if not speed else speed
        skipFactor = 1 # speed when user decides to skip an entry
        if entry.printdate:
            print('\n' + entry.time.ctime(), flush=True)
        for letter, time in zip(entry.text, entry.intervals):
            sleep(time * skipFactor / speed)
            if kbhit() and str(getch())[2:-1] in ['\\r', ' ']:
                skipFactor=0
            print(
                '\b \b' if letter=='\b' else letter,
                end='', flush=True
            )

    def backup(self, name:str=None, *args):
        print('backing up diary%s...'%((' to %s'%name) if name else ''))
        self.diary.backup(name)
        print('COMPLETE')
