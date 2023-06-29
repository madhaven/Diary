class EmergencyStop(Exception):
    '''raised when User presses Ctrl+C during the record of an entry.'''

class BadFileHeader(Exception):
    '''raised when the metadata in the provided diary file does not match any version of FileManagers.'''
