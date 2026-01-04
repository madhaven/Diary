#to remove spaces between timestamp and text in diary records
from os import sep, system
oldDiary = input('Drop your old diary file to read from : ')
newDiary = input('Specify your new file : ')
with open(oldDiary, 'r') as f, open(newDiary, 'w') as f2:
    f2.write('diary v2.10 github.com/madhaven/diary')
    while True:
        line = f.readline()
        if not line: break
        # print('old : ', line)
        line = line[:24]+line[25:]
        # print('new : ', line)
        f2.write('\n\n')
        f2.write(line)

        times = f2.write(str([
            round(float(time), 4)
            for time in f.readline()[1:-2].split(', ')
        ]))
        f.readline()
input('Conversion Complete')
