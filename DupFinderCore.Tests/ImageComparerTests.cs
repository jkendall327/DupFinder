using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace DupFinderCore.Tests
{

    public class ImageComparerTests
    {
        private readonly ImageComparer _sut;
        private readonly ImageComparisonRuleset _rules = new();

        private readonly UserSettings _settings = new()
        { CompareByDate = true, CompareByPixels = true, CompareBySize = true };

        public ImageComparerTests() => _sut = new ImageComparer(_rules);

        [Fact]
        public void ImageIsKept_WhenSuperiorOnAllCriteria()
        {
            var list = new List<(IEntry, IEntry)>();

            var moq1 = new Mock<IEntry>();
            moq1.SetupGet(x => x.Size).Returns(30000); // smaller
            moq1.SetupGet(x => x.Date).Returns(DaysAgo(1)); // newer
            moq1.SetupGet(x => x.Pixels).Returns(1000); // more pixels


            var moq2 = new Mock<IEntry>();
            moq2.SetupGet(x => x.Size).Returns(40000);
            moq2.SetupGet(x => x.Date).Returns(DaysAgo(2));
            moq2.SetupGet(x => x.Pixels).Returns(900);

            list.Add((moq1.Object, moq2.Object));

            _sut.Process(list, _settings);

            Assert.Contains(moq1.Object, _sut.Keep);
            Assert.Contains(moq2.Object, _sut.Trash);
        }

        [Fact]
        public void ImageIsKept_WhenSuperiorOnMoreCriteria()
        {
            var list = new List<(IEntry, IEntry)>();

            var moq1 = new Mock<IEntry>();
            moq1.SetupGet(x => x.Size).Returns(30000); // smaller
            moq1.SetupGet(x => x.Date).Returns(DaysAgo(1));
            moq1.SetupGet(x => x.Pixels).Returns(1000); // more pixels


            var moq2 = new Mock<IEntry>();
            moq2.SetupGet(x => x.Size).Returns(40000);
            moq2.SetupGet(x => x.Date).Returns(DaysAgo(1));
            moq2.SetupGet(x => x.Pixels).Returns(900);

            list.Add((moq1.Object, moq2.Object));

            _sut.Process(list, _settings);

            Assert.Contains(moq1.Object, _sut.Keep);
            Assert.Contains(moq2.Object, _sut.Trash);
        }

        [Fact]
        public void ImagesAreUnsure_WhenImagesAreEquallySuperiorOnDifferentCriteria()
        {
            var list = new List<(IEntry, IEntry)>();

            // moq1 has better size, moq2 has better date

            var moq1 = new Mock<IEntry>();
            moq1.SetupGet(x => x.Size).Returns(20000);
            moq1.SetupGet(x => x.Date).Returns(DaysAgo(2));
            moq1.SetupGet(x => x.Pixels).Returns(1000);


            var moq2 = new Mock<IEntry>();
            moq2.SetupGet(x => x.Size).Returns(30000);
            moq2.SetupGet(x => x.Date).Returns(DaysAgo(1));
            moq2.SetupGet(x => x.Pixels).Returns(1000);

            list.Add((moq1.Object, moq2.Object));

            _sut.Process(list, _settings);

            Assert.Contains(moq1.Object, _sut.Unsure);
            Assert.Contains(moq2.Object, _sut.Unsure);
        }

        [Fact]
        public void ImagesAreUnsure_WhenEqualOnAllCounts()
        {
            var list = new List<(IEntry, IEntry)>();

            var moq1 = new Mock<IEntry>();
            moq1.SetupGet(x => x.Size).Returns(30000);
            moq1.SetupGet(x => x.Date).Returns(DaysAgo(1));
            moq1.SetupGet(x => x.Pixels).Returns(1000);


            var moq2 = new Mock<IEntry>();
            moq2.SetupGet(x => x.Size).Returns(30000);
            moq2.SetupGet(x => x.Date).Returns(DaysAgo(1));
            moq2.SetupGet(x => x.Pixels).Returns(1000);

            list.Add((moq1.Object, moq2.Object));

            _sut.Process(list, _settings);

            Assert.Contains(moq1.Object, _sut.Unsure);
            Assert.Contains(moq2.Object, _sut.Unsure);
        }

        private static DateTime DaysAgo(int days)
        {
            return DateTime.Now.Subtract(TimeSpan.FromDays(days));
        }
    }
}
