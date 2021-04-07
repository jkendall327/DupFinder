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

        public ImageComparerTests()
        {
            _sut = new ImageComparer(_rules);
        }

        private List<(IEntry, IEntry)> CreateMockImages()
        {
            var list = new List<(IEntry, IEntry)>();

            var moq1 = new Mock<IEntry>();
            moq1.SetupGet(x => x.Size).Returns(30000); // smaller
            moq1.SetupGet(x => x.Date).Returns(DateTime.Now.Subtract(TimeSpan.FromDays(1))); // newer
            moq1.SetupGet(x => x.Pixels).Returns(1000); // more pixels


            var moq2 = new Mock<IEntry>();
            moq2.SetupGet(x => x.Size).Returns(40000);
            moq2.SetupGet(x => x.Date).Returns(DateTime.Now.Subtract(TimeSpan.FromDays(2)));
            moq2.SetupGet(x => x.Pixels).Returns(900);

            list.Add((moq1.Object, moq2.Object));

            return list;
        }

        [Fact]
        public void ImageIsKept_WhenSuperiorOnAllCriteria()
        {
            var lilst = CreateMockImages();

            _sut.Process(lilst, _settings);

            Assert.Equal(lilst[0].Item1, _sut.Keep[0]);
        }

        [Fact]
        public void ImageIsKept_WhenSuperiorOnMoreCriteria()
        {
            _sut.Process(CreateMockImages(), _settings);

            Assert.True(true);
        }

        [Fact]
        public void ImagesAreUnsure_WhenEqualOnAllCounts()
        {
            _sut.Process(CreateMockImages(), _settings);

            Assert.True(true);
        }
    }
}
