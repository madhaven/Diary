# How to Contribute

* Create your own fork of the repository / clone the repository
* Make your modifications
* Add your commits with a brief description of what you've done
* Add more details inside if you think your commit needs an explanation  
  This helps reduce further discussion on your commits / PRs
* Push your changes to your own fork and create a Pull Request to merge your changes into my `main`.

## Some Background

The `DiaryService` handles all the logic invovled in managing the records and saving them appropriately.  
It is instantiated and used by the CLI controller or can even be used by WebControllers or the likes.  
This class contains methods to fetch and save and handle all the data involved in the Diary project.  

The `Entry` class contains all the data involved in an entry.  
This includes the local time, the inputted string which contains the backspaces and the time delta on each keypress made by the user which helps to recreate the typing speed of the user.

Diary uses an sqlite DB to save the information, handled by Dotnet EFCore within the Diary.Data project.

## Project Architecture

Diary is built with a modular and extensible architecture, primarily leveraging .NET technologies. The project is structured into several key components:

* **Diary.CLI:** The command-line interface application that users interact with. It parses arguments and orchestrates operations.
* **Diary.Core:** Contains core interfaces and abstractions for the diary's business logic.
* **Diary.Core.Implementation:** Provides concrete implementations for the interfaces defined in `Diary.Core`.
* **Diary.Data:** Handles data persistence, including the `DiaryDbContext` for Entity Framework Core and database migrations.
* **Diary.Models:** Contains the data models used throughout the application.
* **Diary.Tests:** A comprehensive suite of unit and integration tests to ensure the reliability and correctness of the application.

This separation of concerns ensures a maintainable, testable, and scalable application.  