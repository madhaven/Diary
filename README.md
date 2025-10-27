# Diary

Diary is a powerful and intuitive command-line application designed for quick, efficient journaling. Seamlessly log your daily entries, effortlessly search through your memories, and even relive your past thoughts at the exact speed you typed them. With Diary, your personal reflections are always at your fingertips, organized and secure.

## Why Choose Diary?

* Instant Logging & Effortless Organization
	Quickly jot down your thoughts, ideas, and daily events without ever leaving your terminal.  
	Diary makes journaling a natural extension of your workflow with a simple call.  
	```
	diary log
	```
* Relive Every Moment
	Diary's unique "replay" feature allows you to experience your past entries as if you were typing them again.  
	Relive specific days or browse collections of memories with flexible reading options.  
	```
	diary read [YEAR] [Month] [Date]
	diary read today
	diary read yesterday
	```
* Powerful Search, Precise Results
	Never lose a thought again. Diary's robust search functionality helps you pinpoint entries with ease.  
	Use the `--strict` flag to retrieve only those entries that contain all your specified search terms, ensuring highly accurate results.  
	```
	diary [search|find] [string [string2 [...]]] [--strict]
	```
* Your Data, Always Safe**
	With Diary, your valuable entries are always protected.  
	Easily create backup copies of your diary file, giving you peace of mind.
	```
	diary backup [filename]
	```
* Simple Setup
	Add the Diary executable to your system's path for instant access from any directory.  
	Diary intelligently handles its own configuration, creating an `application.json` file if one isn't found, so you can focus on what matters: your thoughts.

## Get Started Today!

Display all available commands and begin your journaling journey with Diary.
```
diary
```

## Contributions

Diary welcome contributions from the community!  
If you're interested in making Diary even better, please refer to our [CONTRIBUTING.md](CONTRIBUTING.md) guide for detailed information on how to get started.