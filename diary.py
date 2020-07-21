try:
    from time import time, sleep, ctime, strftime, localtime
    from datetime import datetime, timedelta
    import sys
    from msvcrt import getch
    from math import floor
    filename = 'D:\\Works\\python\\Diary\\diary.txt'
    version = '2.7_debug'
    testing = False
except Exception as e: input('include error'+str(e))

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
                print(
                    (letter, '\b \b')[letter=='\b'],
                    end='', flush=True
                )
                sleep(time)
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
        if int(record.timestamp[20:]) == int(year) and
                record.timestamp[4:7].lower() == month[:3].lower() and
                int(record.timestamp[8:10]) == int(date):
            record.print()


def readmonth(year, month, file=filename):
    '''Select records from a specific month'''
    for record in readall():
        if record.timestamp[20:] == year and
                record.timestamp[4:7].lower() == month[:3].lower():
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


def log(string, pause):
    '''to log values while testing'''
    if testing: print(string)
    if pause:input()


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
    if arglen == 1: 
        log("running the app cuz no other attribs are specified")
        main()
    elif arglen >= 2:
        if sys.argv[1].lower() == 'version': 
            print("Diary.py v"+version+"\nproduct of Jay Creations")
        elif sys.argv[1].lower() == 'read':
            log("reading diary logs acc to parameters")
            if arglen >= 3:
                if sys.argv[2].lower() == 'yesterday':
                    yesterday = datetime.now() - timedelta(1)
                    readdate(
                        yesterday.strftime('%Y'),
                        yesterday.strftime('%b'),
                        yesterday.strftime('%d')
                    )
                    exit()
                elif sys.argv[2].lower() == 'today':
                    today = datetime.now()
                    readdate(
                        today.strftime('%Y'),
                        today.strftime('%b'),
                        today.strftime('%d')
                    )
                    exit()
                elif strftime('%Y', localtime()) >= sys.argv[2]:
                    if arglen == 3: 
                        if (not set(sys.argv[2]) - set('1234567890')):
                            readyear(sys.argv[2]) #Y
                            exit()
                    elif arglen >= 4:
                        if not bool(set(sys.argv[3]) & set('1234567890')): 
                            if testing:
                                print("month isn't a number")
                                print((int(strftime('%Y', localtime()))*12)+int(strftime('%m', localtime())))
                                print( (int(sys.argv[2])*12)+month(sys.argv[3]))
                                input()
                            if ((int(strftime('%Y', localtime()))*12)+int(strftime('%m', localtime())) >= (int(sys.argv[2])*12)+month(sys.argv[3])):
                                if testing:print("Month and Year are less or equal to todays")
                                if arglen == 4:#YM
                                    readmonth(sys.argv[2], sys.argv[3])
                                    exit()
                                elif arglen == 5:#YMD
                                    if ((int(strftime('%Y', localtime()))*12)+int(strftime('%m', localtime())) == (int(sys.argv[2])*12)+month(sys.argv[3])):
                                        if testing:print("Month and Year are equal to todays")
                                        if strftime('%d', localtime()) < sys.argv[4]:
                                            if testing:print("Date is greater than todays, exiting")
                                            exit()
                                    if testing:print("Reading specific date logs, cuz date is less than than or equal to todays")
                                    readdate(sys.argv[2], sys.argv[3], sys.argv[4])
                                    exit()
                print('Try using the format : read [YYYY [Mmm [DD]]]')
                exit()
            else:
                allentries = readall()
                input('Press Enter to read through '+str(len(allentries))+' diary entries...')
                for x in allentries:
                    x.print()
                exit()
        elif sys.argv[1].lower() in ('search', 'find', 'searchall', 'findall'):
            if arglen >= 3:
                search, count = [], 0
                #build an array of strings to search for
                for x in range(2, arglen):
                    search.append(sys.argv[x])
                allentries = readall()
                
                if sys.argv[1].lower() in ('search', 'find'):
                    for entry in allentries:
                        for y in range(len(search)):
#                            for xx in search[y].lower(): print(xx, end='-');
#                            for xx in entry.tostring().lower(): print(xx, end='-');
                            if search[y].lower() in entry.tostring().lower():
                                print(entry.timestamp,'|',entry.text, end='')
                                count += 1
                                break
                elif sys.argv[1].lower() in ('searchall', 'findall'):
                    for entry in allentries:
                        for y in range(len(search)):
                            if search[y].lower() not in entry.tostring().lower():
                                break
                            elif y == len(search)-1:
                                print(entry.timestamp,'|',entry.text, end='')
                                count+=1
                                
                print(count, "entries found")
            else:
                print('Try using the formats\nsearch [search_text [search_text2 [...]]]\nsearchall [search_text [search_text2 [...]]]\n searchall matches all strings together\nfind and findall also works in a similar fashion')
                
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
