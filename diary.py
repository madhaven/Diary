VERSION = '3.4'
TESTING = VERSION[-5:] == 'debug'
if TESTING: from traceback import print_exc

import calendar
import re
import sys
from abc import abstractmethod
from datetime import datetime, timedelta
from msvcrt import getch, kbhit
from os import path, sep, system
from time import sleep

try:
    try:
        exec(open(sep.join([path.expanduser('~'), 'diary_config']), 'r').read())
        if TESTING: filelocation = 'testdiary'
    except:
        file = input('Specify a location to read/write your Diary file : ')
        filelocation = file.split(sep)+['diary']
        typespeed = 1.25
        try:
            with open(sep.join([path.expanduser('~'), 'diary_config']), 'w') as f:
                f.write('filelocation, typespeed = %s, %s'%(filelocation, typespeed))
        except Exception as e:
            input('There was an error, try a valid filename with write permissions.')
            if TESTING: print(e)
            exit()
    filename = sep.join(filelocation)
    if TESTING: filename='testdiary'
except Exception as e:
    input('include error '+str(e))
    exit()

class EmergencyStop(Exception):
    '''raised when User presses Ctrl+C during the record of an entry.'''
    pass

class BadFileHeader(Exception):
    '''raised when the metadata in the provided diary file does not match any version of FileManagers.'''

class FileManager:
    '''To manage file read/write across different versions'''

    def backup(self, name):
        file = open(self.fileName, 'r')
        back = open(name, 'w')
        for line in file:
            back.write(line)
        file.close()
        back.close()
        return True

    @staticmethod
    def get_manager(fileName:str, preferredFiler:"FileManager"=None):
        '''Returns the registered class for accessing the file and ensure file is initialized.'''
        REGISTERED_FILERS: list[FileManager] = [Filer_2_10, Filer_3_2,] # arranged earliest first

        if preferredFiler and preferredFiler in REGISTERED_FILERS:
            return preferredFiler(fileName)
        # if file don't exist, use the latest File Manager
        if not path.isfile(fileName):
            return REGISTERED_FILERS[-1](fileName)

        # if file exists, use corresponding FileManager
        with open(fileName, 'r') as file:
            meta = file.readline()[:-1]
        for filer in REGISTERED_FILERS:
            log('checking', meta, filer.header_string())
            if meta == filer.header_string():
                log(filer)
                return filer(fileName)
        else:
            # TODO: file don't have a suitable meta, catch error in controller
            raise BadFileHeader
    
    @abstractmethod
    def __init__(self, fileName):
        self.fileName = fileName
    
    @classmethod
    @abstractmethod
    def header_string(self):
        '''returns the header string that should be found at the first line of the file'''

    @abstractmethod
    def load(self) -> list:
        '''Loads data if file exists and header matches.\n
        Raises exception if file don't exist.\n
        Raises BadFileHeader exception if header don't match.'''

    @abstractmethod
    def write(self, *entries):
        '''write entries into file, if file don't exist: Creates a new one'''

class Filer_3_2(FileManager):
    HEADER = 'diary v%s github.com/madhaven/diary'
    VERSION = '3.2'
    ENTRY = '\n%s%s%s\n'

    @classmethod
    def header_string(self):
        return self.HEADER%self.VERSION
    
    def entryString(self, entry:"Entry"):
        return '\n%s%s%s\n'%(
            entry.time.ctime(),
            entry.text,
            str(entry.intervals)
        )

    def load(self) -> list:
        entries = []
        with open(self.fileName, 'r') as file:
            if self.header_string() != file.readline()[:-1]:
                raise BadFileHeader
            file.readline()

            while True:
                text = file.readline()
                if not text:
                    # TODO: escape blank lines / add info for SESSION concept in Diary
                    break
                intervals = [float(time) for time in file.readline()[1:-2].split(', ')]
                file.readline()
                entry = Entry(text[24:], datetime.strptime(text[:24], '%a %b %d %H:%M:%S %Y'), intervals)
                entry.printdate = False if (entries and entries[-1].time.day==entry.time.day) else True
                entries.append(entry)
        return entries
    
    def write(self, *entries):
        with open(self.fileName, 'a') as file:
            if file.tell()==0:
                file.writelines([self.header_string(), '\n'])
            for entry in entries:
                if entry:
                    file.write(self.entryString(entry))

class Filer_2_10(FileManager):
    HEADER = 'diary v2.10 github.com/madhaven/diary'

    @classmethod
    def header_string(self):
        return self.HEADER
    
    def load(self) -> list:
        entries = []
        with open(self.fileName, 'r') as file:
            if self.header_string() != file.readline()[:-1]:
                raise BadFileHeader
            file.readline()

            while True:
                text = file.readline()
                if not text:
                    # skip blank lines / add info for session concept in Diary
                    break
                intervals = [float(time) for time in file.readline()[1:-2].split(', ')]
                file.readline()
                entry = Entry(text[24:], datetime.strptime(text[:24], '%a %b %d %H:%M:%S %Y'), intervals)
                entry.printdate = False if (entries and entries[-1].time.day == entry.time.day) else True
                entries.append(entry)
        return entries

    def write(self, *entries):
        with open(self.fileName, 'a') as file:
            if not file.tell():
                file.writelines([self.header_string()])
            for entry in entries:
                file.write('\n\n%s%s%s'%(entry.time.ctime(), entry.text, entry.intervals))

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

    def __init__(self, filename:str):
        '''initializes the Diary'''
        self.entries = []
        self.filer:FileManager = FileManager.get_manager(filename)
    
    def add(self, *entries):
        '''writes the entry/entries to the file'''
        self.filer.write(*entries)
        
    def load(self):
        '''
        Scans the file specified on creation and initializes the list of Entry objects\n
        This replaces any existing Entries in memory with data from the file\n
        \n
        Only recommended when reading entries as adding entries to the diary do not require any data in memory  
        '''
        self.entries = self.filer.load()

    def filter(self, year:int=None, month:int=None, day:int=None, fetchLatest=False) -> list:
        '''fetches diary records acc to date match, if `fetchLatest` is set to True, entries from the last day is fetched.'''
        self.load()
        if fetchLatest:
            lastDate:datetime = self.entries[-1].time
            year, month, day = lastDate.year, lastDate.month, lastDate.day
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

    def backup(self, name:str=None, *args):
        if not name:
            name = self.filer.fileName
            name = name[:name.rfind(sep)+1] + 'diaryback_'
            name += datetime.now().strftime('%Y%m%d%H%M')
        self.filer.backup(name)

    def export(self, args:str=None):
        '''Handles Export options'''
        if not args:
            print( 
                'not implemented',
                # 'Usage','-----',
                # 'diary export[to] text/txt',
                # 'diary export[to] csv',
                sep = '\n'
            )
            return

class DiaryController():
    '''provide access to Diary'''

    def __init__(self, filename:str, stopWord:str='bye', typespeed:float=1.5):
        self.stopWord:str = stopWord
        self.typespeed:int = typespeed
        try:
            self.diary:Diary = Diary(filename)
        except BadFileHeader as e:
            raise NotImplementedError
        self.version:str = VERSION
    
    def _show_version(self, args=None):
        '''Shows Controller Version'''
        print("Diary.py v" + self.version + "\ngithub.com/madhaven/Diary")

    def _show_info(self):
        '''Shows Manual'''
        self._show_version()
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
                self._show_info()
            elif args[0] in ['log', 'entry']:
                print(self.pre_log_advice())
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
                self._show_version()
            elif args[0] in ['backup']:
                self.backup(*args[1:])
            else:
                self._show_info()
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
                self.print_entry(entry)
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
    
    def print_entry(self, entry:Entry, speed:int=None):
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

    def pre_log_advice(self):
        '''returns a string to be printed before logging an entry'''
        return (
            'Ctrl+C / "bye" to stop recording.\n'
            'Say something memorable about today :)\n'
        )

def log(*args, pause=False, **kwargs):
    '''to log values while testing'''
    if TESTING: 
        print(*args, **kwargs)
        if pause:input()

if __name__ == '__main__':

    #get cli args
    cliargs = [arg.lower() for arg in sys.argv][1:]
    log('DIARY cli args', sys.argv, '->', cliargs)

    try:
        controller = DiaryController(filename, typespeed=typespeed)
        controller.main(*cliargs)
    except Exception as e:
        print(
            'An error crashed the diary :', str(e),
            'please report issues to https://github.com/madhaven/Diary/issues',
            sep='\n', end='\n\n'
        )
        if TESTING: print_exc()
