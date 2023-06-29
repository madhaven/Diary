
import datetime


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
