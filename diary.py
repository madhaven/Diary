try:
    from time import time, sleep, ctime, strftime, localtime
    from datetime import datetime, timedelta
    import sys
    from msvcrt import getch
    from math import floor
    filename='D:\\Works\\python\\Diary\\diary.txt'
    version='2.1_debug'
except Exception as e: input('include error'+str(e))

class entry:
    #class to store each and every entry that the user makes
    
    def __init__(self):
        #initiailization, set all variables to emptyy strings and printdate to False and initialize the filename taken from globals
        self.timestamp = ''
        self.text = ''
        self.time = ''
        self.printdate = False
        self.file = filename
        
    def record(self):
        #method that records an entry, an entry ends when the Return key is pressed
        try:
            entry=[] 
            t=time() #takes a timestamp
            self.timestamp=ctime() #records time as a string
            while True:
                char=str(getch())[2:-1] #takes a string that the user pressed
                t2 = time() #srecords time again
                if (t2-t)>60: t2, t=(t2-t)-int(t2-t)+5, t2
                else: t2, t=t2-t, t2 #formats time
                
                if char=='\\r':
                    #onpress of Return key
                    if len(entry) == 0:
                        continue; #do nothing when the text is empty
                    print() #add a new line
                    entry.append(['\n', t2]) 
                    self.text = ''.join([x[0] for x in entry])
                    self.time = str([floor(x[1]*1000)/1000 for x in entry])
                    #add a new line character to the entry and format the entry object
                    
                    if 'bye' in self.text.lower(): #to specify whether or not to wait for the next entry or to stop.
                        return True
                    else: 
                        return False
                elif char=='\\x08':
                    #onpress of backspace I think 
                    print('\b \b', end='', flush=True) #mame the necassae changes on screen 
                    char='\b'
                elif char=="\\":print("\\", end='', flush=True) #onpress of the slashcharachter 
                else: print(char, end='', flush=True) #normal character
                entry.append([char, t2]) #add character to the entry array.
        except Exception as e: input(e)
            
    def write(self, file=filename):
        #write the current entry to file and reset the object, I think
        with open(file, 'a') as f:
            if f.tell() !=0: f.write('\n\n')
            f.write(self.timestamp+' '+self.text+self.time)
        self.text, self.time = '', ''
        
    def print(self, file=filename):
        #to print the entry. This includes the sleep function to imitate the user's type speed. 
        #self.printdate decides wheter or not to print the timestamp I think
        try:
            if self.printdate:print('\n'+self.timestamp, flush=True)
#            print(self.timestamp, end=' ', flush=True)
            for x in range(len(self.text)):
                if (self.text[x] =='\b'): print('\b \b', end='', flush=True)
                else: print(self.text[x], end='', flush=True)
                sleep(self.time[x])
        except KeyboardInterrupt as e:
            print("\nDiary closed")
            exit()

def readall(file=filename):
    entries=[] #list of entry Objects to be returned to the caller. 
    f=open(filename, 'r') #file to read from, obtained from globals
    while True:
        #parses the file and appends entry objects created from each entry in the file
        text = f.readline()
        if not text: break;
        e = entry()
        e.text = text[25:]
        e.timestamp = text[:24]
        if len(entries) == 0: e.printdate = True
        elif entries[len(entries)-1].timestamp[4:10]+entries[len(entries)-1].timestamp[20:24] != e.timestamp[4:10]+e.timestamp[20:24]:
#            print(entries[len(entries)-1].timestamp[4:10]+entries[len(entries)-1].timestamp[20:24]+' and '+e.timestamp[4:10]+e.timestamp[20:])
            e.printdate = True
        time = f.readline()[1:-2].split(', ')
        #int(time[y].split('.')[0])+int(time[y].split('.')[1])/len(time[y].split('.')[1]) 
        e.time = [ float(time[y]) for y in range(len(time))]
        f.readline()
        entries.append(e)
#        input(e.printdate)
    f.close()
    return entries
def readdate(year, month, date, file=filename):
    for x in readall():
        if int(x.timestamp[20:]) == int(year) and x.timestamp[4:7].lower() == month.lower() and int(x.timestamp[8:10]) == int(date):
            x.print()
def readmonth(year, month, file=filename):
    for x in readall():
        if x.timestamp[20:] == year and x.timestamp[4:7].lower() == month.lower():
            x.print()
def readyear(year, file=filename):
    for x in readall():
        if x.timestamp[20:] == year:
            x.print()

def month(mon):
    #function to return the monthnumber to the monthstring
    month = {'jan':1,
             'feb':2,
             'mar':3,
             'apr':4,
             'may':5,
             'jun':6,
             'jul':7,
             'aug':8,
             'sep':9,
             'oct':10,
             'nov':11,
             'dec':12}
    return month[mon[:3].lower()]

def main():
    try:
        ent = entry()
        while True:
            stoprequest = ent.record()
            ent.write(filename)
            if stoprequest: break
    except Exception as e: input(str(e)+'\n'+'Contact @_mad_haven')

if __name__ == '__main__':
    if len(sys.argv) == 1: 
        #just run the app cuz no other attribs are specified
        main()
    elif len(sys.argv) >= 2:
        if sys.argv[1] == 'version': 
            #print the versions of the app;
            print("Diary.py v"+version+"\nproduct of Jay Creations")
        elif sys.argv[1] == 'read':
            #read diary logs acc to following parameters
            if len(sys.argv) >= 3:
                if sys.argv[2].lower() == 'yesterday':
                    yesterday = datetime.now()-timedelta(1)
                    readdate(yesterday.strftime('%Y'), yesterday.strftime('%b'), yesterday.strftime('%d'))
                elif sys.argv[2].lower() == 'today':
                    today = datetime.now()
                    readdate(today.strftime('%Y'), today.strftime('%b'), today.strftime('%d'))
                elif strftime('%Y', localtime()) >= sys.argv[2]:
                    if len(sys.argv) == 3: 
                        if (not set(sys.argv[2]) - set('1234567890')):
                            readyear(sys.argv[2]) #Y
                    elif len(sys.argv) >= 4:
                        if not bool(set(sys.argv[3]) & set('1234567890')):
#                            print((int(strftime('%Y', localtime()))*12)+int(strftime('%m', localtime())))
#                            print( (int(sys.argv[2])*12)+month(sys.argv[3]))
#                            input()
                            if ((int(strftime('%Y', localtime()))*12)+int(strftime('%m', localtime())) >= (int(sys.argv[2])*12)+month(sys.argv[3])):
                                if len(sys.argv) == 4:#YM
                                    readmonth(sys.argv[2], sys.argv[3])
                                    exit()
                                elif len(sys.argv) == 5:#YMD
                                    if strftime('%d', localtime()) >= sys.argv[4]:
                                        readdate(sys.argv[2], sys.argv[3], sys.argv[4])
                                    exit()
                else:print('Try using the format : read [YYYY [Mmm [DD]]]')
                exit()
            else:
                allentries=readall()
                input('Press Enter to read through '+str(len(allentries))+' diary entries...')
                for x in allentries:
                    x.print()
                exit()
        elif sys.argv[1] == 'export':
            if len(sys.argv) == 3:
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
