VERSION = '3.2'
TESTING = VERSION[-5:] == 'debug'
if TESTING:
    from traceback import print_exc

# default configs
STOPWORD = 'bye'
TYPESPEED = 1.5