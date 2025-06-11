import sys
from datetime import datetime
from os import path, sep

from DiaryConfig import *
from filers.file_manager import FileManager
from filers.filer_factory import FilerFactory
from filers.filer_3_2 import Filer_3_2
from diary_controller import DiaryController

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


class Diary:
    '''
    class to handle all Diary interactions
    '''

    def __init__(self, filename:str):
        '''initializes the Diary'''
        self.entries = []
        self.filer:FileManager = FilerFactory.getManager('3.2')(filename)
    
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
