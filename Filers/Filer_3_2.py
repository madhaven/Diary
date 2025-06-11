import datetime
from DiaryExceptions import BadFileHeader
from Entry import Entry
from filers.file_manager import FileManager


class Filer_3_2(FileManager):
    HEADER = 'diary v%s github.com/madhaven/diary'
    VERSION = '3.2'
    ENTRY = '\n%s%s%s\n'

    @classmethod
    def headerString(self):
        return self.HEADER%self.VERSION
    
    def entryString(self, entry:"Entry"):
        return '\n%s%s%s\n'%(
            entry.time.ctime(),
            entry.text,
            str(entry.intervals)
        )

    def load(self) -> list:
        entries = []
        with open(self.fileName, 'r') as file:
            if self.headerString() != file.readline()[:-1]:
                raise BadFileHeader
            file.readline()

            while True:
                text = file.readline()
                if not text:
                    # TODO: escape blank lines / add info for SESSION concept in Diary
                    break
                intervals = [float(time) for time in file.readline()[1:-2].split(', ')]
                file.readline()
                entry = Entry(text[24:], datetime.strptime(text[:24], '%a %b %d %H:%M:%S %Y'), intervals)
                entry.printdate = False if (entries and entries[-1].time.day==entry.time.day) else True
                entries.append(entry)
        return entries
    
    def write(self, *entries):
        with open(self.fileName, 'a') as file:
            if file.tell()==0:
                file.writelines([self.headerString(), '\n'])
            for entry in entries:
                if entry:
                    file.write(self.entryString(entry))
