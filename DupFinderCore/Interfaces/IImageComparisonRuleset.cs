using System;
using System.Collections.Generic;
using DupFinderCore.Enums;

namespace DupFinderCore.Interfaces
{
    /// <summary>
    /// Compares two <see cref="Entry"/> items to determine which is superior.
    /// </summary>
    public interface IImageComparisonRuleset
    {
        /// <summary>
        /// List of methods that will compare two <see cref="Entry"/> items and return a <see cref="Judgement"/> indicating which is superior.
        /// </summary>
        List<Func<IEntry, IEntry, Judgement>> Rules { get; }

        /// <summary>
        /// Read a <see cref="UserSettings"/> object to dynamically add rules for image comparison.
        /// </summary>
        /// <param name="settings"></param>
        void Configure();
    }
}
