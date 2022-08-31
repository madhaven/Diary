
from datetime import datetime
import os
from unittest import TestCase
from diary import Diary, Entry


class EntryTest(Entry):

    def __init__(self, maxCount, stopWord, text: str, time: datetime, intervals: list = None, printdate: bool = False):
        super().__init__(text, time, intervals, printdate)
        self.maxCount = maxCount
        self.stopWord = stopWord
        self.count = 0
        self.text = '%s\n'%text

    def record(self):
        if self.count >= self.maxCount:
            self.text, self.intervals = self.stopWord+'\n', [.1 for _ in range(len(self.stopWord)+1)]
        else:
            self.intervals = [.1 for _ in range(len(self.text))]
            self.count += 1
        self.printdate = False
        return self

class EntryExceptionTest(EntryTest):

    def record(self):
        raise KeyboardInterrupt

class Test_Diary(TestCase):
    '''Conducts unit tests for diary methods'''

    def setUp(self) -> None:
        self.filename = 'test.txt'
        if os.path.isfile(self.filename):
            os.remove(self.filename)
        return super().setUp()
    
    def tearDown(self) -> None:
        if os.path.isfile(self.filename):
            os.remove(self.filename)
        return super().tearDown()        

    def test_init(self):
        diary = Diary(self.filename, version='3.2')
        self.assertEqual(diary.entries, [], 'Expected blank list of diary entries on init')
        self.assertEqual(diary.file, self.filename, 'Filename not saved into file attribute on init')
        self.assertEqual(diary.printdate, False, 'printdate expected to be False')
        self.assertEqual(diary.typespeed, 1.5, 'Typespeed mismatch')
        self.assertEqual(diary.sessionStopWord, 'bye', 'stop Word mismatch')
        self.assertEqual(diary.version, '3.2', 'version mismatch')
    
    def test_blank_init(self):
        self.assertRaises(TypeError, Diary, msg='Filename parameter missing')

    def test_add_brand_new(self):
        diary = Diary(self.filename)
        nowTime = datetime.now()
        diary.add(Entry('hello\n', nowTime, [0.1 for _ in range(5)]))
        f = open(self.filename, 'r').read()
        self.assertEqual(f, 'diary v'+diary.version+' github.com/madhaven/diary\n\n' + nowTime.ctime() + 'hello\n[0.1, 0.1, 0.1, 0.1, 0.1]', 'Brand New entry format error')
    
    def test_add_entry(self):
        diary = Diary(self.filename)
        
        nowTime = datetime.now()
        diary.add(Entry('hello', nowTime, [0.1 for _ in range(5)]))
        nowTime2 = datetime.now()
        diary.add(Entry('olleh', nowTime2, [0.2 for _ in range(5)]))
        expected = 'diary v%s github.com/madhaven/diary\n\n%shello[0.1, 0.1, 0.1, 0.1, 0.1]\n\n%solleh[0.2, 0.2, 0.2, 0.2, 0.2]'%(
            diary.version, nowTime.ctime(), nowTime2.ctime())

        fileContents = open(self.filename, 'r').read()
        self.assertEqual(fileContents, expected, 'Next entry format error')

    def test_record(self):
        time = datetime.now()
        ctime = time.ctime()
        diary = Diary(self.filename, stopWord='bye')
        testEntryMaker = EntryTest(2, 'bye', 'bleh', time)
        diary.record(testEntryMaker)

        expected = '%s\n\n%sbleh\n[0.1, 0.1, 0.1, 0.1, 0.1]\n\n%sbleh\n[0.1, 0.1, 0.1, 0.1, 0.1]\n\n%sbye\n[0.1, 0.1, 0.1, 0.1]'%(diary.headerFormat%diary.version, ctime, ctime, ctime)
        fileContents = open(self.filename, 'r').read()

        self.assertEqual(fileContents, expected)

    def test_record_plenty(self):
        n = 1000
        time = datetime.now()
        ctime = time.ctime()
        diary = Diary(self.filename, stopWord='bye')
        testEntryMaker = EntryTest(n, 'bye', 'bleh', time)
        diary.record(testEntryMaker)

        expected = diary.headerFormat%diary.version
        for _ in range(n):
            expected += '\n\n%sbleh\n[0.1, 0.1, 0.1, 0.1, 0.1]'%ctime
        expected += '\n\n%sbye\n[0.1, 0.1, 0.1, 0.1]'%ctime
        fileContents = open(self.filename, 'r').read()

        self.assertEqual(fileContents, expected)
    
    # def test_record_keyboard_interrupt(self):

    def test_read_empty_args(self):
        diary = Diary(self.filename)
        self.assertEqual(diary.read([]), None)
    
    def test_read(self):
        pass
    
    def test_load(self):
        pass

    def test_search(self):
        pass

    def test_export(self):
        pass