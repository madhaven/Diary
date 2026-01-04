#f=open(input('Enter old diary file name : '), 'r')
from math import floor
from random import random
f=open('diary.txt', 'r')
f2=open('diary', 'w')

count = 0
while f.readline():
    count+=1
input('total lines : '+str(count)+', press enter to continue')
f.close()

print('PROGRESS : 00%', end='')
f=open('diary.txt', 'r')
countc = 0
while True:
    try:
        x=f.readline()
        countc+=1
        if countc/count > 1: break
        if len(x)==25 and x[13]==x[16]==':':
            y=f.readline()
            if y[:7]=='PROCESS': continue
            z=[floor(random()*15)/100 for x in y]
            f2.write(x[:-1]+' '+y+str(z)+'\n\n')
        print('\b\b\b'+str(int(countc*100/count))+'%', end='')
    except Exception as e: 
        if input(e)=='q': break
    finally: pass
f.close()
f2.close()
