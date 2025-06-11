import datetime
from DiaryExceptions import BadFileHeader
from Entry import Entry
from filers.file_manager import FileManager


class Filer_2_10(FileManager):
    HEADER = 'diary v2.10 github.com/madhaven/diary'

    @classmethod
    def headerString(self):
        return self.HEADER
    
    def load(self) -> list:
        entries = []
        with open(self.fileName, 'r') as file:
            if self.headerString() != file.readline()[:-1]:
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
                file.writelines([self.headerString()])
            for entry in entries:
                file.write('\n\n%s%s%s'%(entry.time.ctime(), entry.text, entry.intervals))
