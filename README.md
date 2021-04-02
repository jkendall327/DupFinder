# DupFinder
WPF .NET 5 app for detecting duplicate images.

Made as a way to get familiar with DI, .NET Core logging and WPF databinding. 

Main points of interest for the image-comparison are ImageComparer.cs and ImageComparisonRuleset.cs.

Planned features:
* Use of euclidian distance in addition to PHash to calculate image similarity.
* Alternative to automatic image filtering where the user can pick which of two images to keep.
* Command-line functionality.
