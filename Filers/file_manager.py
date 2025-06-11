
from abc import abstractmethod
from os import path

class FileManager:
    '''To manage file read/write across different versions'''

    @abstractmethod
    def __init__(self, fileName: str):
        
        # if file don't exist, use the latest File Manager
        if not path.isfile(fileName):
            return cls.REGISTERED_FILER(fileName)

        # if file exists, use corresponding FileManager
        with open(fileName, 'r') as file:
            meta = file.readline()[:-1]
        for filer in cls.REGISTERED_FILER:
            # log('checking', meta, filer.headerString())
            if meta == filer.headerString():
                # log(filer)
                return filer(fileName)
        else:
            # TODO: file don't have a suitable meta, catch error in controller
            raise BadFileHeader

    def backup(self, name):
        file = open(self.fileName, 'r')
        back = open(name, 'w')
        for line in file:
            back.write(line)
        file.close()
        back.close()
        return True

    @classmethod
    def registerFiler(cls, filer):
        '''Registers a list of Filer classes inheritted from FileManager.
        This serves for a dependency injection mechanism.'''
        cls.REGISTERED_FILER = filer
    
    @abstractmethod
    def __init__(self, fileName):
        self.fileName = fileName
    
    @classmethod
    @abstractmethod
    def headerString(self):
        '''returns the header string that should be found at the first line of the file'''

    @abstractmethod
    def load(self) -> list:
        '''Loads data if file exists and header matches.\n
        Raises exception if file don't exist.\n
        Raises BadFileHeader exception if header don't match.'''

    @abstractmethod
    def write(self, *entries):
        '''write entries into file, if file don't exist: Creates a new one'''
