# DupFinder
WPF .NET 5 app for detecting duplicate images using PHash and euclidian distance comparisons.

Made as a way to get familiar with DI, .NET Core logging and WPF databinding. 

Main points of interest for the image-comparison are ImageComparer.cs and ImageComparisonRuleset.cs.

Planned features:
* Use of euclidian distance in addition to PHash to calculate image similarity.
* Alternative to automatic image filtering where the user can pick which of two images to keep.
* Command-line functionality.

Known bugs/points to improve:
* Race conditions/deadlocks when dealing with non-disposed image handles.
* High memory usage. Not tested on large numbers of files.
* No visual feedback on files being moved and potential overwritten.
* Processor.cs is likely an unnecessary facade class.
