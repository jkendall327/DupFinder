using System;
using System.Collections.Generic;
using DupFinderCore.Interfaces;
using DupFinderCore.Models;
using DupFinderCore.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace DupFinderCore.Tests
{

    public class ImageComparerTests
    {
        private readonly ImageComparer _sut;
        private readonly ImageComparisonRuleset _rules;
        readonly List<Pair> list = new();

        readonly Mock<IEntry> Left = new();
        readonly Mock<IEntry> Right = new();
        readonly Mock<IEntry> Good = new();

        private readonly UserSettings _settings = new()
        { CompareByDate = true, CompareByPixels = true, CompareBySize = true };

        public ImageComparerTests()
        {
            _rules = new(_settings);
            _sut = new ImageComparer(_rules, null);

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

            list.Add(new Pair(Good.Object, Right.Object));

            _sut.Compare(list);

            _sut.Keep.Should().Contain(Good.Object);
            _sut.Trash.Should().Contain(Right.Object);
        }

        [Fact]
        public void ImageIsKept_WhenSuperiorOnMoreCriteria()
        {
            Right.SetupGet(x => x.Size).Returns(40000);
            Right.SetupGet(x => x.Date).Returns(DaysAgo(1));
            Right.SetupGet(x => x.Pixels).Returns(900);

            list.Add(new Pair(Good.Object, Right.Object));

            _sut.Compare(list);

            _sut.Keep.Should().Contain(Good.Object);
            _sut.Trash.Should().Contain(Right.Object);
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

            list.Add(new Pair(Left.Object, Right.Object));

            _sut.Compare(list);

            _sut.Unsure.Should().Contain(Left.Object);
            _sut.Unsure.Should().Contain(Right.Object);
        }

        [Fact]
        public void ImagesAreUnsure_WhenEqualOnAllCounts()
        {
            Right.SetupGet(x => x.Size).Returns(30000);
            Right.SetupGet(x => x.Date).Returns(DaysAgo(1));
            Right.SetupGet(x => x.Pixels).Returns(1000);

            list.Add(new Pair(Good.Object, Right.Object));

            _sut.Compare(list);

            _sut.Unsure.Should().Contain(Good.Object);
            _sut.Unsure.Should().Contain(Right.Object);
        }

        private static DateTime DaysAgo(int days)
        {
            return DateTime.Now.Subtract(TimeSpan.FromDays(days));
        }
    }
}
