# Diary
A CLI diary app. Requires you to specify your diary document in code and set up environment variables to access it from your terminal

After having added the .py extension and the program directory to path, the Diary interface could be called with one of three ways to start it up.

the ```diary``` command lets you log in at the current time and save it to the file specified IN CODE // work pending
```diary read [YEAR [Mon [ DT]]]
diary read 2018
diary read 2020 Jan
diary read 2020 Jan 30
diary read today
diary read yesterday
```
helps you to read specific days or collections of days.

<br>```diary version``` helps you checkout what version Diary is running on.
<br>```diary search [string [string2 [...]]]``` helps you search for strings in your file. The ```searchall``` also works in a similar fashion except that it fetches those entries that simultaneously have all the search strings in it.
