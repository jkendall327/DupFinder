namespace DupFinderCore.Models
{
    /// <summary>
    /// A simple data object that represents the user's desired criteria when comparing <see cref="IEntry"/> items.
    /// </summary>
    public class UserSettings
    {
        /// <summary>
        /// Whether or not <see cref="IEntry"/> items should be compared based on date -- newer items are better.
        /// </summary>
        public bool CompareByDate { get; set; }

        /// <summary>
        /// Whether or not <see cref="IEntry"/> items should be compared based on filesize -- smaller items are better.
        /// </summary>
        public bool CompareBySize { get; set; }

        /// <summary>
        /// Whether or not <see cref="IEntry"/> items should be compared based on pixel count -- more is better.
        /// </summary>
        public bool CompareByPixels { get; set; }
    }
}