from datetime import datetime
from unittest import TestCase, skip
from Entry import Entry

class TestEntry(TestCase):
    '''conducts unit tests for entry methods'''

    def test_init(self):
        t = datetime.now()
        entry = Entry('hello\n', t, [0.1 for _ in range(6)])
        self.assertEqual(entry.intervals, [0.1 for _ in range(6)], msg="intervals should match")
        self.assertEqual(entry.printdate, False, msg='Printdate should match')
        self.assertEqual(entry.time, t,msg='Entry time should match')
        self.assertEqual(Entry().text, '', msg='Blank Entry text should be blank')
        self.assertEqual(Entry().intervals, [], msg='Blank Entry intervals should be blank')
        self.assertEqual(len(entry.text), len(entry.intervals), msg='Intervals should correspond to text in the entry')

    def test_str(self):
        entry = Entry('hello\b\b\b\bmy name is x')
        self.assertEqual(str(entry), 'hmy name is x', msg='string conversion does not match')
    
    def test_str_with_backspace(self):
        entry = Entry('x\b\b\b\b\bhello')
        self.assertEqual(str(entry), 'hello', msg='string conversion does not match')

    @skip
    def test_record(self):
        pass
        # TODO: find way to inject streams

    @skip
    def test_print(self):
        pass
        # TODO: find way to inject streams