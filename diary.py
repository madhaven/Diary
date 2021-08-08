try:
    from time import time, sleep, ctime, strftime, localtime
    from datetime import datetime, timedelta
    import sys
    from os import sep
    from msvcrt import getch
    from math import floor
    version = '2.9'
    testing = False
except Exception as e: input('include error'+str(e))
try: from info import *
except: filename = sep.join(['D:', 'desktop', 'diary'])

class entry:
    '''
    class to store an entry that the user makes
    '''
    
    def __init__(self, text='', timestamp='', printdate=False):
        #initiailization, set all variables to emptyy strings and printdate to False and initialize the filename taken from globals
        self.text = text
        self.timestamp = timestamp
        self.time = ''
        self.printdate = printdate
        self.file = filename
    
    
    def record(self):
        '''
        method that records an entry, an entry ends when the Return key is pressed
        returns True to wait for next entry, False otherwise
        '''
        try:
            entry = []
            #record time as a timestamp and as string
            t, self.timestamp = time(), ctime()
            
            while True:
                #get pressed character
                char=str(getch())[2:-1]
                
                #format time
                t2 = time()
                if (t2-t)>60: t2, t=(t2-t)-int(t2-t)+5, t2
                else: t2, t = t2-t, t2
                
                #Return key pressed
                if char=='\\r':
                    
                    #do nothing if text is empty
                    if len(entry) == 0: continue
                        
                    #add a new line character to the entry and format the entry object
                    entry.append(['\n', t2]) 
                    self.text = ''.join([x[0] for x in entry])
                    self.time = str([floor(x[1]*1000)/1000 for x in entry])
                    
                    print()
                    
                    #specify if or not to wait for the next entry
                    return 'bye' in self.text.lower()
                    
                #Escape seq pressed
                elif char == '\\\\':
                    #print single \ instead of \\
                    print('\\', end='', flush=True)
                    char = "\\"
                    
                #Tabspace pressed
                elif char == '\\t':
                    #print tabspace instead of \t literally
                    print('\t', end='', flush=True)
                    char = '\t'
                    
                #Backspace pressed, I think 
                elif char == '\\x08':
                    #make the necassare changes on screen 
                    print('\b \b', end='', flush=True)
                    char = '\b'
                    
                #normal character?
                else:
                    print(char, end='', flush=True)
                    
                #add character to entry list.
                entry.append([char, t2])
                
        except Exception as e: input(e)
    
    
    def write(self, file=filename):
        '''
        write the current entry to file and reset the object, I think
        '''
        with open(file, 'a') as f:
            #reset file if empty
            if f.tell() != 0: f.write('\n\n')
            f.write(self.timestamp+' '+self.text+self.time)
        self.text = self.time = ''
    
    
    def print(self, file=filename):
        '''
        prints the entry.
        Contains sleep() to imitate the user's type speed.
        self.printdate decides whether or not to print the timestamp I think
        '''
        try:
            if self.printdate: print('\n'+self.timestamp, flush=True)
            for letter, time in zip(self.text, self.time):
                sleep(time)
                print(
                    (letter, '\b \b')[letter=='\b'],
                    end='', flush=True
                )
#            print(self.timestamp, end=' ', flush=True)
#            for x in range(len(self.text)):
#                if self.text[x] == '\b':
#                    print('\b \b', end='', flush=True)
#                else:
#                    print(self.text[x], end='', flush=True)
        except KeyboardInterrupt as e:
            print("\nDiary closed")
            exit()
    
    
    def tostring(self):
        '''
        Convert user input information and returns data alone.
        used in search ops where backspace characters need not be considered
        in the entry
        '''
        text = ''
        for x in self.text:
            text = (text+x, text[:-1])[x=='\b']
#            if x == '\b': text = text[:-1]
#            else: text = text + x
        return text;
            

def readall(file=filename):
    '''Scan diary file and returns a list of entry objects'''
    #list of entry Objects to be returned to the caller. 
    entries=[]
    
    #file to read from, obtained from globals
    f=open(filename, 'r')
    
    while True:
        #scans file, appends entry objects created from each file entry
        text = f.readline()
        
        #escape blank lines
        if not text: break
        e = entry(text[25:], text[:24])
        if len(entries) == 0 or entries[-1].timestamp[4:10]+entries[-1].timestamp[20:24] != e.timestamp[4:10]+e.timestamp[20:24]:
#            print(entries[len(entries)-1].timestamp[4:10]+entries[len(entries)-1].timestamp[20:24]+' and '+e.timestamp[4:10]+e.timestamp[20:])
            e.printdate = True
        time = f.readline()[1:-2].split(', ')
#        int(time[y].split('.')[0])+int(time[y].split('.')[1])/len(time[y].split('.')[1]) 
        e.time = [ float(time[y]) for y in range(len(time))]
        f.readline()
        entries.append(e)
#        input(e.printdate)
    f.close()
    return entries


def readdate(year, month, date, file=filename):
    '''Select records from a specific date'''
    for record in readall():
        if (
                int(record.timestamp[20:]) == int(year) and
                record.timestamp[4:7].lower() == month[:3].lower() and
                int(record.timestamp[8:10]) == int(date)):
            record.print()


def readmonth(year, month, file=filename):
    '''Select records from a specific month'''
    for record in readall():
        if (
                record.timestamp[20:] == year and
                record.timestamp[4:7].lower() == month[:3].lower()):
            record.print()


def readyear(year, file=filename):
    '''Select records from a specific Year'''
    for record in readall():
        if record.timestamp[20:] == year:
            record.print()


def month(mon):
    '''returns the monthnumber to the monthstring'''
    return {
        [
            'jan', 'feb', 'mar', 
            'apr', 'may', 'jun',
            'jul', 'aug', 'sep',
            'oct', 'nov', 'dec'
        ][x]:x+1 for x in range(12)
    }[mon[:3].lower()]


def log(string, pause=False):
    '''to log values while testing'''
    if testing: 
        if type(string) == str:
            print(string)
        else:
            for one in string:
                print(one)
        if pause:input()
andpause = True


def diaryversion():
    '''Shows version'''
    print("Diary.py v"+version+"\nproduct of github.com/madhaven")



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


def main():
    '''Fired if user wishes to add a new diary entry'''
    try:
        ent = entry()
        while True:
            stopRec = ent.record()
            ent.write(filename)
            if stopRec: break
    except Exception as e:
        input(str(e)+'\n'+'Contact github.com/madhaven')

if __name__ == '__main__':
    log(sys.argv)
    arglen = len(sys.argv)
    
    #no cli arguments
    if arglen == 1:
        diaryinfo()
    
    #one or more arguments
    elif arglen >= 2:
        
        #diary log or entry option
        if sys.argv[1].lower() in ['log', 'entry']:
            log("running the app cuz no other attribs are specified")
            main()
        
        #help option
        if sys.argv[1].lower() == '/?':
            diaryinfo()
        
        #version option
        elif sys.argv[1].lower() == 'version':
            diaryversion()
        
        #read option
        elif sys.argv[1].lower() == 'read':
            log("reading diary logs acc to parameters")
            
            #one or more Read-arguments
            if arglen >= 3:
                
                #read today or yesterday
                if sys.argv[2].lower() in ('yesterday', 'today'):
                    day = (
                        datetime.now(), datetime.now()-timedelta(1)
                    )[sys.argv[2].lower() == 'yesterday']
                    readdate(
                        day.strftime('%Y'),
                        day.strftime('%b'),
                        day.strftime('%d')
                    )
                    exit()
                
                #if requested year is not from future
                elif sys.argv[2] <= strftime('%Y', localtime()):
                    
                    #read Year
                    if arglen == 3:
                        if sys.argv[2].isdigit():
                            readyear(sys.argv[2])
                            exit()
                    
                    #two or more Read-arguments
                    elif arglen >= 4:
                        
                        #if month don't have numbers in it
                        if sys.argv[3].isalpha(): 
                            log("month isn't a number", andpause)
                            
                            #if query month and year are valid
                            if ((int(strftime('%Y', localtime()))*12)+int(strftime('%m', localtime())) >= (int(sys.argv[2])*12)+month(sys.argv[3])):
                                log("Month and Year are less or equal to todays")
                                
                                #read Month
                                if arglen == 4:
                                    readmonth(sys.argv[2], sys.argv[3])
                                    exit()
                                
                                #read Date
                                elif arglen == 5:
                                    
                                    #if query date is more than todays
                                    if ((int(strftime('%Y', localtime()))*12)+int(strftime('%m', localtime())) == (int(sys.argv[2])*12)+month(sys.argv[3])):
                                        log("Month and Year are equal to todays")
                                        if strftime('%d', localtime()) < sys.argv[4]:
                                            log("Date is greater than todays, exit-ing")
                                            exit()
                                    log("Reading specific date logs, cuz date is less than than or equal to todays")
                                    readdate(sys.argv[2], sys.argv[3], sys.argv[4])
                                    exit()
                
                #argument errors / missing elses route here
                print('usage', '-----', 'diary read [YYYY [Mon [DD]]]', sep='\n')
                exit()
            
            #no Read-arguments
            else:
                allentries = readall()
                if 'q' in input('Press Q to skip '+str(len(allentries))+' diary entries...').lower():
                    exit()
                for x in allentries: x.print()
                exit()
        
        #Search/Find option
        elif sys.argv[1].lower() in ('search', 'find', 'searchall', 'findall'):
            
            #one or more Search arguments
            if arglen >= 3:
                #build an array of strings to search for
                search, count = [], 0
                for x in range(2, arglen):
                    search.append(sys.argv[x])
                allentries = readall()
                
                #loose search
                if sys.argv[1].lower() in ('search', 'find'):
                    for entry in allentries:
                        for y in range(len(search)):
#                            for xx in search[y].lower(): print(xx, end='-');
#                            for xx in entry.tostring().lower(): print(xx, end='-');
                            if search[y].lower() in entry.tostring().lower():
                                print(entry.timestamp,'|',entry.tostring(), end='')
                                count += 1
                                break
                
                #strict search
                elif sys.argv[1].lower() in ('searchall', 'findall'):
                    for entry in allentries:
                        for y in range(len(search)):
                            if search[y].lower() not in entry.tostring().lower():
                                break
                            elif y == len(search)-1:
                                print(entry.timestamp,'|',entry.tostring(), end='')
                                count+=1
                                
                print(count, "entries found")
            
            #no search arguments
            else:
                print(
                    'usage','-----',
                    'diary search [search_text [search_text2 [...]]]',
                    'diary searchall [search_text [search_text2 [...]]]',
                    'diary searchall matches all strings together',
                    'PS : find and findall also works',
                    sep = '\n'
                )
                
        #Export option ?
        elif sys.argv[1] == 'export':
            if arglen == 3:
                if (sys.argv[2] == 'txt') or (sys.argv[2] == 'text'):
                    fn = input('Filename to save as : ')
                    f = open(fn, 'w')
                    for x in readall():
                        for y in range(len(x.text)):
                            try:
                                if x.text[y]=='\b':
                                    x.text=x.text[:y-1]+x.text[y+1:]
                                    y-=1
                            except Exception: pass
                        f.write(x.timestamp+' '+x.text)
                f.close()
        