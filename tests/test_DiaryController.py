import os
from unittest import TestCase, skip

from diary import Diary, DiaryController

class Test_DiaryController(TestCase):
    '''unit tests for DiaryController'''

    def setUp(self) -> None:
        self.filename = 'test.txt'
        if os.path.isfile(self.filename):
            os.remove(self.filename)
        return super().setUp()
    
    def tearDown(self) -> None:
        if os.path.isfile(self.filename):
            os.remove(self.filename)
        return super().tearDown()
    
    def test_blank_init(self):
        self.assertRaises(TypeError, DiaryController, msg='blank filename error not raised')
    
    def test_init(self):
        diary = Diary(self.filename)
        controller = DiaryController(diary, 'bye', 1.5)
        self.assertEqual(controller.diary.file, 'test.txt', msg='filename not initialized')
        self.assertEqual(controller.typespeed, 1.5, msg='Typespeed mismatch')
        self.assertEqual(controller.stopWord, 'bye', msg='stop Word mismatch')
        self.assertEqual(diary.printdate, False, 'printdate expected to be False')
    
    @skip
    def test_record_keyboard_interrupt(self):
        pass

    @skip
    def test_unknown_month_interrupt(self):
        pass