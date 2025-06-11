
from datetime import date, datetime, timedelta
import os
from unittest import TestCase, expectedFailure, skip
from diary import Diary
from Entry import Entry
from Filers import Filer_2_10, Filer_3_2


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

    @expectedFailure
    def test_init(self):
        diary = Diary(self.filename, version='3.2')
        self.assertEqual(diary.entries, [], 'Expected blank list of diary entries on init')
        self.assertEqual(diary.file, self.filename, 'Filename not saved into file attribute on init')
        self.assertEqual(diary.version, '3.2', 'version mismatch')
    
    def test_blank_init(self):
        self.assertRaises(TypeError, Diary, msg='Filename parameter missing')

    def test_add_brand_new(self):
        diary = Diary(self.filename)
        nowTime = datetime.now()
        diary.add(Entry('hello\n', nowTime, [0.1 for _ in range(6)]))
        f = open(self.filename, 'r').read()
        if type(diary.filer) == type(Filer_2_10):
            diary.version = '2.10'
        elif type(diary.filer) == type(Filer_3_2):
            diary.version = '3.2'
        else:
            raise Exception('nofiler')
        # self.assertEqual(f, 'diary v'+diary.version+' github.com/madhaven/diary\n\n' + nowTime.ctime() + 'hello\n[0.1, 0.1, 0.1, 0.1, 0.1, 0.1]', 'Brand New entry format error')
        self.assertEqual(f, 'diary v'+diary.version+' github.com/madhaven/diary\n\n' + nowTime.ctime() + 'hello\n[0.1, 0.1, 0.1, 0.1, 0.1, 0.1]\n', 'Brand New entry format error')
    
    def test_add_entry(self):
        diary = Diary(self.filename)
        
        nowTime = datetime.now()
        diary.add(Entry('hello', nowTime, [0.1 for _ in range(5)]))
        nowt2 = datetime.now()
        diary.add(Entry('olleh', nowt2, [0.2 for _ in range(5)]))
        expected = 'diary v3.2 github.com/madhaven/diary\n\n%shello[0.1, 0.1, 0.1, 0.1, 0.1]\n\n%solleh[0.2, 0.2, 0.2, 0.2, 0.2]\n'%(
            nowTime.ctime(), nowt2.ctime())

        fileContents = open(self.filename, 'r').read()
        self.assertEqual(fileContents, expected, 'Next entry format error')

    def test_add_plenty(self):
        n = 1000
        time = datetime.now()
        ctime = time.ctime()
        diary = Diary(self.filename)
        diary.add(*[Entry('bleh\n', time, [.1]*5)]*n)

        expected = diary.filer.headerString() + '\n'
        for _ in range(n):
            expected += '\n%sbleh\n[0.1, 0.1, 0.1, 0.1, 0.1]\n'%ctime

        fileContents = open(self.filename, 'r').read()
        self.assertEqual(fileContents, expected, 'Diary saved an unexpected output')
    
    def test_load(self):
        f = open(self.filename, 'w')
        f.write('diary v3.2 github.com/madhaven/diary\n\nSun Sep 11 19:05:01 2022hello \n[0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1]\n\nSun Sep 11 19:05:03 2022testing\n[0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1]\n\nSun Sep 11 19:05:04 2022bye\n[0.1, 0.1, 0.1, 0.1]\n')
        f.close()

        diary = Diary(self.filename)
        diary.load()
        self.assertListEqual(diary.entries, [
            Entry('hello \n', datetime.strptime('Sun Sep 11 19:05:01 2022', '%a %b %d %H:%M:%S %Y'), [0.1 for _ in range(7)]),
            Entry('testing\n', datetime.strptime('Sun Sep 11 19:05:03 2022', '%a %b %d %H:%M:%S %Y'), [0.1 for _ in range(8)]),
            Entry('bye\n', datetime.strptime('Sun Sep 11 19:05:04 2022', '%a %b %d %H:%M:%S %Y'), [0.1 for _ in range(4)]),
        ])

    @skip
    def test_load_versions(self):
        pass

    def test_filter_empty_args(self):
        diary = Diary(self.filename)
        e = Entry('bleh\n', datetime.now(), [0.1]*5)
        diary.add(e)
        self.assertEqual(diary.filter() == [e], True, msg='Diary should return None if no args are supplied')
    
    def test_filter_year_alone(self):
        diary = Diary(self.filename)
        t1, t2, t3 = datetime.now(), datetime.now(), datetime.now()
        t1 -= timedelta(days=-30)
        t2 -= timedelta(days=-70)
        t3 -= timedelta(days=-1000)
        e1, e2, e3 = Entry('bleh\n', t1, [0.1]*5), Entry('bleh\n', t2, [0.1]*5), Entry('bleh\n', t3, [0.1]*5)
        diary.add(e1, e2, e3)
        self.assertListEqual([e1, e2], diary.filter(year=t1.year), msg='expected entries of a year were not returned')

    def test_filter_month_alone(self):
        diary = Diary(self.filename)
        t1, t2, t3 = datetime.now(), datetime.now(), datetime.now()
        t1 -= timedelta(days=-365)
        t3 -= timedelta(days=-1000)
        e1, e2, e3 = Entry('bleh\n', t1, [0.1]*5), Entry('bleh\n', t2, [0.1]*5), Entry('bleh\n', t3, [0.1]*5)
        diary.add(e1, e2, e3)
        self.assertListEqual([e1, e2], diary.filter(month=t1.month), msg='expected entries of a month were not returned')
    
    def test_filter_date_alone(self):
        diary = Diary(self.filename)
        t1, t2, t3 = datetime(2015, 5, 7), datetime(2016, 7, 7), datetime(2018, 2, 8)
        e1, e2, e3 = Entry('bleh\n', t1, [0.1]*5), Entry('bleh\n', t2, [0.1]*5), Entry('bleh\n', t3, [0.1]*5)
        diary.add(e1, e2, e3)
        self.assertListEqual([e1, e2], diary.filter(day=t1.day), msg='expected entries of a month were not returned')
    
    def test_filter_year_and_month(self):
        diary = Diary(self.filename)
        t1, t2, t3, t4 = datetime(2015, 5, 7), datetime(2015, 5, 9), datetime(2018, 5, 8), datetime(2015, 8, 1)
        e1, e2, e3, e4 = Entry('bleh\n', t1, [0.1]*5), Entry('bleh\n', t2, [0.1]*5), Entry('bleh\n', t3, [0.1]*5), Entry('bleh\n', t4, [0.1]*5)
        diary.add(e1, e2, e3, e4)
        self.assertListEqual([e1, e2], diary.filter(year=t1.year, month=t1.month), msg='expected entries of a month were not returned')
    
    def test_filter_year_and_date(self):
        diary = Diary(self.filename)
        t1, t2, t3, t4 = datetime(2015, 5, 7), datetime(2015, 9, 7), datetime(2018, 5, 7), datetime(2015, 8, 6)
        e1, e2, e3, e4 = Entry('bleh\n', t1, [0.1]*5), Entry('bleh\n', t2, [0.1]*5), Entry('bleh\n', t3, [0.1]*5), Entry('bleh\n', t4, [0.1]*5)
        diary.add(e1, e2, e3, e4)
        self.assertListEqual([e1, e2], diary.filter(year=t1.year, day=t1.day), msg='expected entries of a month were not returned')
    
    def test_filter_month_and_date(self):
        diary = Diary(self.filename)
        t1, t2, t3, t4 = datetime(2015, 5, 7), datetime(2018, 5, 7), datetime(2019, 5, 1), datetime(2015, 8, 7)
        e1, e2, e3, e4 = Entry('bleh\n', t1, [0.1]*5), Entry('bleh\n', t2, [0.1]*5), Entry('bleh\n', t3, [0.1]*5), Entry('bleh\n', t4, [0.1]*5)
        diary.add(e1, e2, e3, e4)
        self.assertListEqual([e1, e2], diary.filter(month=t1.month, day=t1.day), msg='expected entries of a month were not returned')

    @skip
    def test_filter_unrecognized_month(self):
        pass
    
    def test_search(self):
        pass

    @skip
    def test_export(self):
        pass