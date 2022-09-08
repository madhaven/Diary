try:
    version = '3.2'
    testing = version[-5:] == 'debug'
    if testing: import traceback
    from os import sep, path, system
    import sys

    from time import sleep
    from datetime import datetime, timedelta
    from msvcrt import getch, kbhit
    import re, calendar

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
    pass

class Entry:
    '''stores an entry that the user makes'''
    
    def __init__(self, text:str='', time:datetime=None, intervals:list=None, printdate:bool=False):
        '''set empty strings'''
        self.text:str = text
        self.time:datetime = time
        self.intervals:list = intervals
        self.printdate:bool = printdate
    
    def __bool__(self):
        return len(self.text) > 0
    
    def __str__(self):
        '''
        Convert user input information and returns data alone\n
        used in search ops where backspace characters need not be considered\n
        in the entry
        '''
        text = ''
        for x in self.text:
            if x == '\b': text = text[:-1]
            else: text += x
        return text

    def record(self):
        '''method that records an entry, an entry ends when the Return key is pressed'''
        try:
            entry = []
            #record current time
            self.time = datetime.now()
            t1 = self.time.timestamp()

            while True:
                #get pressed character
                char = str(getch())[2:-1]
                
                #format time
                t2 = datetime.now().timestamp()
                if (t2-t1) > 60:
                    t2, t1 = (t2-t1) - int(t2-t1) + 5, t2
                else:
                    t2, t1 = t2 - t1, t2
                t2 = round(t2, 4)
                
                #Return key
                if char == '\\r':
                    
                    #do nothing if text is empty
                    if len(entry) == 0:
                        continue
                        
                    #add a new line character to the entry and format the entry object
                    entry.append(['\n', t2])
                    self.text = ''.join(list(zip(*entry))[0])
                    self.intervals = list(list(zip(*entry))[1])
                    print()
                    return self
                    
                #Ctrl+C
                elif char == '\\x03':
                    if len(entry) != 0:
                        entry.append(['\n', t2])
                        self.text = ''.join(list(zip(*entry))[0])
                        self.intervals = list(list(zip(*entry))[1])
                    raise EmergencyStop

                #Escape char
                elif char == '\\\\':
                    print('\\', end='', flush=True)
                    char = "\\"
                    
                #Tabspace
                elif char == '\\t':
                    print('\t', end='', flush=True)
                    char = '\t'
                    
                #Backspace? 
                elif char == '\\x08':
                    print('\b \b', end='', flush=True)
                    char = '\b'
                
                #stray characters
                elif len(char) > 1:
                    char = str(getch())[2:-1]
                    continue
                    
                #normal character?
                else:
                    print(char, end='', flush=True)
                    
                #add character to entry list.
                entry.append([char, t2])
                
        except Exception as e: 
            print(e)
            raise e
    
    def print(self, speed=1.5):
        '''
        prints the entry\n
        Contains sleep() to imitate the user's type speed\n
        self.printdate decides whether or not to print the timestamp\n
        '''
        skipFactor = 1
        if self.printdate: print('\n'+self.time.ctime(), flush=True)
        for letter, time in zip(self.text, self.intervals):
            sleep(time*skipFactor/speed)
            if kbhit() and str(getch())[2:-1] in ['\\r', ' ']: skipFactor=0
            print(
                '\b \b' if letter=='\b' else letter,
                end='', flush=True
            )

class Diary():
    '''
    class to handle all Diary interactions
    '''
    headerFormat = 'diary v%s github.com/madhaven/diary'
    entryFormat = '\n\n%s%s%s'
    version = version

    def __init__(self, filename:str, typespeed:float=1.5, stopWord:str='bye', version:str=version):
        '''initializes the Diary'''
        self.entries = []
        self.file = filename
        self.printdate = False
        self.typespeed = typespeed
        self.sessionStopWord = stopWord
    
    def add(self, *entries):
        '''writes the entry/entries to the file'''
        with open(self.file, 'a') as f:
            if f.tell()==0:
                f.writelines([self.headerFormat%self.version])
            for entry in entries:
                if entry:
                    f.write(self.entryFormat%(entry.time.ctime(), entry.text, str(entry.intervals)))
    
    def record(self, entry:Entry=Entry()):
        '''
        To add diary entries to the file\n
        Initiates a loop of Entry records\n
        Loop ends when the stop word is found in an Entry\n
        `entry` is an injected variable that defaults to `Entry` instance
        '''
        try:
            while True:
                entry.record()
                self.add(entry)
                if self.sessionStopWord in str(entry).lower():
                    break
        except EmergencyStop as e:
            self.add(entry)
        except Exception as e:
            print('your last entry : '+str(entry))
            raise e

    def read(self, args:list):
        '''displays the diary entries of the queried day'''
        if not args:
            print( 
                'usage','-----','diary read all','diary read today',
                'diary read yesterday','diary read [YYYY] [Mon] [DD]', sep='\n'
            )
            return
        
        if args[0]=='all':
            year, month, day = None, None, None
        elif args[0] in ['yesterday', 'today']:
            args = datetime.now()-timedelta(1) if args[0]=='yesterday' else datetime.now()
            year, month, day = args.year, args.month, args.day
        else:
            try:
                year = list(filter(re.compile(r'^\d{4}$').match, args))
                year = int(year[0]) if year else None
                monthname = list(filter(re.compile(r'^[a-zA-Z]{3}.*$').match, args))
                monthname = monthname[0] if monthname else None
                month = {month[:3].lower(): index for index, month in enumerate(calendar.month_abbr) if month}[monthname[:3]]
                day = list(filter(re.compile(r'^\d{1,2}$').match, args))
                day = int(day[0]) if day else None
            except:
                print("That date doesn't look right")
                return
        
        self.load()
        readlist = [
            entry for entry in self.entries
            if ((not year or year==entry.time.year) and
                (not month or month==entry.time.month) and
                (not day or day==entry.time.day))
        ]
        print('found '+str(len(readlist))+' entries,')
        try: 
            for entry in readlist: entry.print()
        except KeyboardInterrupt as e:
            print("\nDiary closed")
            return
        
    def load(self):
        '''
        Scans the file specified on creation and returns a list of Entry objects\n
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
                        #escape blank lines / add info for SESSION concept in Diary
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
            return self
        except FileNotFoundError as e:
            print("\nYou do not have a diary file at %s. Make sure your file location is configured properly."%self.file)
            raise e
    
    def search(self, args, strictMode:bool=False):
        '''Search/Find keywords'''
        if not args:
            print( 
                'usage','-----',
                'diary search [search_text [search_text2 [...]]]',
                'diary searchall [search_text [search_text2 [...]]]',
                'diary searchall matches all strings together',
                'PS : find and findall also works the same',
                sep = '\n'
            )
            return

        self.load()
        result_count = 0
        if strictMode:
            for entry in self.entries:
                for i, s in enumerate(args):
                    if s.lower() not in str(entry).lower(): break
                    elif i == len(args) - 1:
                        result_count += 1
                        print(entry.time.strftime('%Y %b %d %H:%M:%S %a'), '|', str(entry), end='')
        else:
            for entry in self.entries:
                for i, s in enumerate(args):
                    if s.lower() in str(entry).lower():
                        result_count += 1
                        print(entry.time.strftime('%Y %b %d %H:%M:%S %a'), '|', str(entry), end='')
                        break
        print(result_count, "entries found")
    
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


def log(*args, pause=False, **kwargs):
    '''to log values while testing'''
    if testing: 
        print(*args, **kwargs)
        if pause:input()
        

def diaryversion(args=None):
    '''Shows version'''
    print("Diary.py v"+version+"\ngithub.com/madhaven/Diary")


def diaryinfo():
    '''Shows Manual'''
    diaryversion()
    print(
        '\nUsage','-----',
        'diary log|entry - to add to your diary',
        'diary version - to check which version your diary is running',
        'diary read - to access older entries or logs that you have made',
        'diary search - to search for keywords',
        'diary export - to export your entries to portable formats | NOT AVAILABLE',
        sep='\n'
    )


if __name__ == '__main__':

    #get cli args
    cliargs = [arg.lower() for arg in sys.argv][1:]
    log('DIARY cli args', sys.argv, '->', cliargs)
    diary = Diary(filename=filename, typespeed=typespeed)

    # parse cli args to select menu
    try:
        if not cliargs: diaryinfo()
        elif cliargs[0] in ['log', 'entry']: diary.record()
        elif cliargs[0] in ['read', 'show']: diary.read(cliargs[1:])
        elif cliargs[0] in ['search', 'find']: diary.search(cliargs[1:])
        elif cliargs[0] in ['searchall', 'findall', 'search all', 'find all']: diary.search(cliargs[1:], True)
        elif cliargs[0] in ['export as', 'export to', 'export']: diary.export(cliargs[1:])
        elif cliargs[0] in ['version', '--version']: diaryversion()
        else: diaryinfo()
    except Exception as e:
        print(
            'An error crashed the diary :', str(e),
            'please report issues to https://github.com/madhaven/Diary/issues',
            sep='\n', end='\n\n'
        )
        if testing: traceback.print_exc()
