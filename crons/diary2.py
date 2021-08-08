try:
    from time import time, sleep, ctime, strftime, localtime
    from datetime import datetime, timedelta
    import sys
    from msvcrt import getch
    from math import floor
    filename='D:\\Works\\python\\Diary\\diary.txt'
    version='1.9'
except Exception as e: input('include error'+str(e))

class entry:
    def __init__(self):
        self.timestamp = ''
        self.text = ''
        self.time = ''
        self.printdate = False
        self.file = filename
    def record(self):
        try:
            word=[]
            t=time() #takes a timestamp
            self.timestamp=ctime()
            while True:
                char=str(getch())[2:-1]
                t2 = time()
                if (t2-t)>60: t2, t=(t2-t)-int(t2-t)+5, t2
                else: t2, t=t2-t, t2
                
                if char=='\\r':
                    print()
                    word.append(['\n', t2])
                    self.text = ''.join([x[0] for x in word])
                    self.time = str([floor(x[1]*1000)/1000 for x in word])
                    if 'bye' in self.text.lower(): return True
                    return False
                elif char=='\\x08':
                    print('\b \b', end='', flush=True)
                    char='\b'
                elif char=="\\":print("\\", end='', flush=True)
                else: print(char, end='', flush=True)
                word.append([char, t2])
        except Exception as e: input(e)
    def write(self, file=filename):
        with open(file, 'a') as f:
            if f.tell() !=0: f.write('\n\n')
            f.write(self.timestamp+' '+self.text+self.time)
        self.text, self.time = '', ''
    def print(self, file=filename):
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

def readdate(year, month, date, file=filename):
    for x in readall():
        if x.timestamp[20:] == year and x.timestamp[4:7].lower() == month.lower() and x.timestamp[8:10] == date:
            x.print()
def readmonth(year, month, file=filename):
    for x in readall():
        if x.timestamp[20:] == year and x.timestamp[4:7].lower() == month.lower():
            x.print()
def readyear(year, file=filename):
    for x in readall():
        if x.timestamp[20:] == year:
            x.print()
def readall(file=filename):
    entries=[]
    try: f=open(filename, 'r')
    except: exit()
    while True:
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
def month(mon):
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
            bye = ent.record()
            ent.write(filename)
            if bye: break
    except Exception as e: input(str(e)+'\n'+'Contact @_mad_haven')

if __name__ == '__main__':
    if len(sys.argv) == 1:
        main()
    elif len(sys.argv) >= 2:
        if sys.argv[1] == 'read':
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
