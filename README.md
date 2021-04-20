# DupFinder
WPF .NET 5 app for detecting duplicate images using PHash and euclidian distance comparisons.

Made as a way to get familiar with DI, .NET Core logging and WPF databinding. 

Planned features:
* Alternative to automatic image filtering where the user can pick which of two images to keep.
* Command-line functionality.

Known bugs/points to improve:
* Lack of unit tests.
* Possible concurrency errors and race conditions (joy!)
* The 'Process Images' button becomes active as soon as any images are loaded and not when the entire set is loaded.
* The logbox doesn't automatically scroll when new entires are added.
* The program has no icon.
* All pairs are marked as 'Unsure'.
