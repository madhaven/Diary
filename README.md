# Diary
The diary app is intended to be called from a command prompt and can be used to quickly log entries of your day and search for past entries.
The read option enables you to replay entries the way you typed it into the console.  
The setup is quite clumsy for a standalone script.

After having added the .py extension and the program directory to path, the Diary interface could be called with one of three ways to start it up.

>```
>diary
>```
> Display the available commands

>```
>diary log
>```
>this command lets you log in at the current time and save it to the file specified (IN CODE)

>```
>diary read [YEAR] [Mon] [Date]
>diary read today
>diary read yesterday
>```
>To read specific days or collections of days.

>```
>diary version
>```
>helps you checkout what version Diary is running on.

>```
>diary    [search | find]     [ string [ string2 [...]]]
>diary [searchall | findall]  [ string [ string2 [...]]]
>```
>helps you search for strings in your file. The ```searchall / findall``` also works in a similar fashion except that it fetches those entries that simultaneously have all the search strings in it.

>```
>diary backup [filename]
>```
>creates a backup copy of the diary file.

The `DiaryController` class is responsible for handling user interaction to access the `Diary`.  
The `Diary` class contains a list of `Entry` instances that contain a record of the user's input.
The `FileManager` class deals with read and write operations of files.
`DiaryController` is passed an instance of the `Diary`.
                    