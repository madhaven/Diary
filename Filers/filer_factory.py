from DiaryExceptions import BadFileHeader
import filers.file_manager as file_manager

from filers.filer_2_10 import Filer_2_10
from filers.filer_3_2 import Filer_3_2


class FilerFactory:

    @classmethod
    def getManager(cls, preferredFiler:str=None):
        '''Returns the registered class for accessing the file and ensure file is initialized.'''

        match preferredFiler:
            case '2.10':
                return Filer_2_10
            case '3.2' | _ :
                return Filer_3_2