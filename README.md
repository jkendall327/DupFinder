# DupFinder
WPF .NET 5 app for detecting duplicate images using PHash and euclidian distance comparisons.

Made as a way to get familiar with DI, .NET Core logging and WPF databinding. 

Planned features:
* Alternative to automatic image filtering where the user can pick which of two images to keep.
* Command-line functionality.

Known bugs/points to improve:
* Lack of unit tests.
* The logbox is hard to read because filenames aren't truncated.
* The logbox doesn't automatically scroll when new entires are added.
* The program has no icon.
* All pairs are marked as 'Unsure'.
