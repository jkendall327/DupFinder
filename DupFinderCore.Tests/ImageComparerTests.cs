using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace DupFinderCore.Tests
{

    public class ImageComparerTests
    {
        private readonly ImageComparer _sut;
        private readonly ImageComparisonRuleset _rules;
        readonly List<(IEntry, IEntry)> list = new();

        readonly Mock<IEntry> Left = new();
        readonly Mock<IEntry> Right = new();

        readonly Mock<IEntry> Good = new();

        private readonly UserSettings _settings = new()
        { CompareByDate = true, CompareByPixels = true, CompareBySize = true };

        public ImageComparerTests()
        {
            _rules = new(_settings);
            _sut = new ImageComparer(_rules);

            Good.SetupGet(x => x.Size).Returns(30000); // smaller
            Good.SetupGet(x => x.Date).Returns(DaysAgo(1)); // newer
            Good.SetupGet(x => x.Pixels).Returns(1000); // more pixels
        }

        [Fact]
        public void ImageIsKept_WhenSuperiorOnAllCriteria()
        {
            Right.SetupGet(x => x.Size).Returns(40000);
            Right.SetupGet(x => x.Date).Returns(DaysAgo(2));
            Right.SetupGet(x => x.Pixels).Returns(900);

            list.Add((Good.Object, Right.Object));

            _sut.Compare(list);

            Assert.Contains(Good.Object, _sut.Keep);
            Assert.Contains(Right.Object, _sut.Trash);
        }

        [Fact]
        public void ImageIsKept_WhenSuperiorOnMoreCriteria()
        {
            Right.SetupGet(x => x.Size).Returns(40000);
            Right.SetupGet(x => x.Date).Returns(DaysAgo(1));
            Right.SetupGet(x => x.Pixels).Returns(900);

            list.Add((Good.Object, Right.Object));

            _sut.Compare(list);

            Assert.Contains(Good.Object, _sut.Keep);
            Assert.Contains(Right.Object, _sut.Trash);
        }

        [Fact]
        public void ImagesAreUnsure_WhenImagesAreEquallySuperiorOnDifferentCriteria()
        {
            // moq1 has better size, moq2 has better date

            Left.SetupGet(x => x.Size).Returns(20000);
            Left.SetupGet(x => x.Date).Returns(DaysAgo(2));
            Left.SetupGet(x => x.Pixels).Returns(1000);

            Right.SetupGet(x => x.Size).Returns(30000);
            Right.SetupGet(x => x.Date).Returns(DaysAgo(1));
            Right.SetupGet(x => x.Pixels).Returns(1000);

            list.Add((Left.Object, Right.Object));

            _sut.Compare(list);

            Assert.Contains(Left.Object, _sut.Unsure);
            Assert.Contains(Right.Object, _sut.Unsure);
        }

        [Fact]
        public void ImagesAreUnsure_WhenEqualOnAllCounts()
        {
            Right.SetupGet(x => x.Size).Returns(30000);
            Right.SetupGet(x => x.Date).Returns(DaysAgo(1));
            Right.SetupGet(x => x.Pixels).Returns(1000);

            list.Add((Good.Object, Right.Object));

            _sut.Compare(list);

            Assert.Contains(Good.Object, _sut.Unsure);
            Assert.Contains(Right.Object, _sut.Unsure);
        }

        private static DateTime DaysAgo(int days)
        {
            return DateTime.Now.Subtract(TimeSpan.FromDays(days));
        }
    }
}
