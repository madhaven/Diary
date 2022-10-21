# How to Contribute

* Create your own fork of the repository / clone the repository
* Make your modifications
* Add your commits with a brief description of what you've done
* Add more details inside if you think your commit needs an explanation  
  This helps reduce further discussion on your commits / PRs
* Push your changes to your own fork and create a Pull Request to merge your changes into my `main`.

## Some Background

The diary project is a CLI interface to log entries.  
It consists of a single `.py` file for ease of use in a local environment, this could be a bad practice but I still prefer it that way.  
The `DiaryController` class handles all user interaction. If you're interested in a UI version of the project, do consider replicating the methods contained in this class.  
The `main` method inside the `DiaryController` handles all arguments from the command line and controls the execution of the program.  

The `Diary` class handles all the logic invovled in managing the records and saving them appropriately.  
It is instantiated and used by the `DiaryController`.  
This class contains methods to fetch and save and handle all the data involved in the Diary project.  

The `FileManager` class is an abstract class that needs to be extended by any class intending to handle file reads and writes.  
This class contains the logic to read and write data according to file standards.  
As of now everything is saved in plain text. This could be changed to a more encrypted fashion later on.

The `Entry` class contains all the data involved in an entry.  
This includes the time delta between each keypress made by the user which helps to recreate the typing speed of the user.