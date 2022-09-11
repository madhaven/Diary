    version = '3.2'
testing = version[-5:] == 'debug'
if testing: import traceback

from os import sep, path, system
import sys
from abc import abstractmethod
from time import sleep
from datetime import datetime, timedelta
from msvcrt import getch, kbhit
import re, calendar

try:
    try:
        exec(open(sep.join([path.expanduser('~'), 'diary_config']), 'r').read())
    except:
        file = input('Specify a location to read/write your Diary file : ')
        filelocation = file.split(sep)+['diary']
        typespeed = 1.25
        try:
            with open(sep.join([path.expanduser('~')]+['diary_config']), 'w') as f:
                f.write('filelocation, typespeed = '+str(filelocation)+', '+str(typespeed))
        except Exception as e:
            input('There was an error, try a valid filename')
            if testing: print(e)
            exit()
    filename = sep.join(filelocation)
    if testing: filename='diary'
    
except Exception as e: 
    input('include error '+str(e))
    exit()

class EmergencyStop(Exception):
    '''raised when User presses Ctrl+C during the record of an entry.'''
    def __init__(self, *args: object) -> None:
        self.entry = args[0]
        super().__init__(args[1:])

class Entry:
    '''stores an entry that the user makes'''
    
    def __init__(self, text:str='', time:datetime=None, intervals:list=[], printdate:bool=False):
        '''set empty strings'''
        self.text:str = text
        self.time:datetime = time
        self.printdate:bool = printdate
        if intervals and len(intervals)==len(text):
            self.intervals:list = intervals
        else:
            self.intervals:list = [0 for _ in range(len(text))]
    
    def __bool__(self) -> bool:
        return len(self.text) > 0
    
    def __eq__(self, __o: "Entry") -> bool:
        if self.text != __o.text:
            return False
        for x, y in zip(self.intervals, __o.intervals):
            if x != y:
                return False
        return True
    
    def __str__(self):
        '''
        Convert user input information and returns data alone\n
        used in search ops where backspace characters need not be considered\n
        in the entry
        '''
        text = ''
        for x in self.text:
            text = text[:-1] if x == '\b' else text + x
        return text
    
    def addChar(self, char:str, time:float):
        '''adds another character to the entry.\n
        the time attribute saves the time taken before/after the keypress.'''
        self.text += char
        self.intervals += [time]

class Diary:
    '''
    class to handle all Diary interactions
    '''
    headerFormat = 'diary v%s github.com/madhaven/diary\n'
    entryFormat = '\n%s%s%s\n'
    version = version

    def __init__(self, filename:str, version:str=version):
        '''initializes the Diary'''
        self.entries = []
        self.file = filename # TODO: add check for file
        self.printdate = False
    
    def add(self, *entries):
        '''writes the entry/entries to the file'''
        with open(self.file, 'a') as f:
            if f.tell()==0:
                f.writelines([self.headerFormat%self.version])
            for entry in entries:
                if entry:
                    f.write(self.entryFormat%(entry.time.ctime(), entry.text, str(entry.intervals)))
        
    def load(self):
        '''
        Scans the file specified on creation and initializes the list of Entry objects\n
        This replaces any existing Entries in memory with data from the file\n
        \n
        Only recommended when reading entries as adding entries to the diary do not require any data in memory  
        '''
        entries = []
        try:
            with open(self.file, 'r') as f:
                if 'diary' in f.readline().split():
                    #version management
                    f.readline()

                while True:
                    text = f.readline()
                    if not text:
                        # TODO: escape blank lines / add info for SESSION concept in Diary
                        break
                    intervals = [float(time) for time in f.readline()[1:-2].split(', ')]
                    f.readline()

                    entry = Entry(text[24:], datetime.strptime(text[:24], '%a %b %d %H:%M:%S %Y'), intervals)
                    if (
                        len(entries) == 0 or 
                        entries[-1].time.day != entry.time.day
                    ): entry.printdate = True
                    entries.append(entry)
            self.entries = entries
            
        except FileNotFoundError as e:
            print("\nYou do not have a diary file at %s. Make sure your file location is configured properly."%self.file)
            raise e

    def filter(self, year:int=None, month:int=None, day:int=None) -> list:
        '''fetches diary records acc to date match'''
        self.load()
        return [
            entry for entry in self.entries
            if ((not year or year==entry.time.year) and
                (not month or month==entry.time.month) and
                (not day or day==entry.time.day))
        ]
    
    def search(self, *args, strictMode:bool=False) -> list:
        '''Search/Find keywords, returns a list of Entry objects'''

        results = []
        self.load()
        if strictMode:
            for entry in self.entries:
                entry_str = str(entry).lower()
                for i, s in enumerate(args):
                    if s.lower() not in entry_str:
                        break
                    elif i == len(args) - 1:
                        results.append(entry)
        else:
            for entry in self.entries:
                entry_str = str(entry).lower()
                for i, s in enumerate(args):
                    if s.lower() in entry_str:
                        results.append(entry)
                        break
        return results

    def export(self, args:str=None):
        '''Handles Export options'''
        if not args:
            print( 
                'not implemented',
                # 'usage','-----',
                # 'diary export[to] text/txt',
                # 'diary export[to] csv',
                sep = '\n'
            )
            return

class DiaryController():
    '''provide access to Diary'''

    def __init__(self, diary:Diary, stopWord:str='bye', typespeed:float=1.5):
        self.stopWord:str = stopWord
        self.typespeed:int = typespeed
        self.diary:Diary = diary
        self.version:str = version
    
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
            'diary export - to export your entries to portable formats | NOT AVAILABLE',
            sep='\n'
        )

    def main(self, *args):
        '''Main entry point into Diary. parses cli args to select menu'''

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
        else:
            self._showInfo()
    
    def log(self):
        '''
        To record diary entries\n
        Initiates a loop of Entry records\n
        Loop ends when the stop word is found in an Entry\n
        `entry` is an injected variable that defaults to `Entry` instance
        '''
        try:
            while True:
                entry = self.record()
                self.diary.add(entry)
                if self.stopWord in str(entry).lower():
                    break
        except EmergencyStop as e:
            self.diary.add(e.entry)
            system('cls')
        except Exception as e:
            print('your last entry was broken: %s'%entry)
            raise e
    
    def record(self) -> Entry:
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
                'usage',
                '-----',
                'diary read all',
                'diary read today',
                'diary read yesterday',
                'diary read [YYYY] [Mon] [DD]',
                sep='\n'
            )
            return
        
        if args[0]=='all':
            year, month, day = None, None, None
        elif args[0] in ['yesterday', 'today']:
            args = datetime.now()-timedelta(days=1) if args[0]=='yesterday' else datetime.now()
            year, month, day = args.year, args.month, args.day
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
            except:
                print("That date doesn't look right")
                if testing: traceback.print_exc()
                return
        
        readlist = self.diary.filter(year, month, day)
        print('found %s entries,'%str(len(readlist)))
        try:
            for entry in readlist:
                self.printEntry(entry)
        except KeyboardInterrupt as e:
            print("\nDiary closed")
            return
    
    def search(self, *args, strictMode:bool=False):
        '''Search/Find keywords'''
        if not args:
            print( 
                'usage',
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
        print(count, "%s found"%("entries" if count>1 else "entry"))
    
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


def log(*args, pause=False, **kwargs):
    '''to log values while testing'''
    if testing: 
        print(*args, **kwargs)
        if pause:input()


if __name__ == '__main__':

    #get cli args
    cliargs = [arg.lower() for arg in sys.argv][1:]
    log('DIARY cli args', sys.argv, '->', cliargs)
    diary = Diary(filename=filename)
    controller = DiaryController(diary, typespeed=typespeed)

    try:
        controller.main(*cliargs)
    except Exception as e:
        print(
            'An error crashed the diary :', str(e),
            'please report issues to https://github.com/madhaven/Diary/issues',
            sep='\n', end='\n\n'
        )
        if testing: traceback.print_exc()