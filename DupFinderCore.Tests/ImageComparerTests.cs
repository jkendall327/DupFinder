using System;
using System.Collections.Generic;
using DupFinderCore.Interfaces;
using DupFinderCore.Models;
using DupFinderCore.Services;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;

namespace DupFinderCore.Tests
{
    public class ImageComparerTests
    {
        private readonly ImageComparer _sut;
        private readonly ImageComparisonRuleset _rules;

        readonly List<Pair> Entries = new();

        readonly Mock<IEntry> Left = new();
        readonly Mock<IEntry> Right = new();
        readonly Mock<IEntry> Good = new();

        private readonly UserSettings _settings = new()
        {
            CompareByDate = true,
            CompareByPixels = true,
            CompareBySize = true
        };

        public ImageComparerTests()
        {
            _rules = new(_settings);
            _sut = new ImageComparer(_rules, NullLogger<ImageComparer>.Instance);

            SetupMock(mock: Good, size: 30000, date: DaysAgo(1), pixels: 1000);
        }

        private void SetupMock(Mock<IEntry> mock, int size, DateTime date, int pixels)
        {
            mock.SetupGet(x => x.Size).Returns(size);
            mock.SetupGet(x => x.Date).Returns(date);
            mock.SetupGet(x => x.Pixels).Returns(pixels);
        }

        private static DateTime DaysAgo(int days)
        {
            return DateTime.Now.Subtract(TimeSpan.FromDays(days));
        }

        [Fact]
        public void ImageIsKept_WhenSuperiorOnAllCriteria()
        {
            SetupMock(mock: Right, size: 40000, date: DaysAgo(2), pixels: 900);

            Entries.Add(new Pair(Good.Object, Right.Object));

            _sut.Compare(Entries);

            _sut.Keep.Should().Contain(Good.Object);
            _sut.Trash.Should().Contain(Right.Object);
        }

        [Fact]
        public void ImageIsKept_WhenSuperiorOnMoreCriteria()
        {
            SetupMock(mock: Right, size: 40000, date: DaysAgo(1), pixels: 900);

            Entries.Add(new Pair(Good.Object, Right.Object));

            _sut.Compare(Entries);

            _sut.Keep.Should().Contain(Good.Object);
            _sut.Trash.Should().Contain(Right.Object);
        }

        [Fact]
        public void ImagesAreUnsure_WhenImagesAreEquallySuperiorOnDifferentCriteria()
        {
            // moq1 has better size, moq2 has better date
            SetupMock(mock: Left, size: 20000, date: DaysAgo(2), pixels: 1000);
            SetupMock(mock: Right, size: 30000, date: DaysAgo(1), pixels: 1000);

            Entries.Add(new Pair(Left.Object, Right.Object));

            _sut.Compare(Entries);

            _sut.Unsure.Should().Contain(Left.Object);
            _sut.Unsure.Should().Contain(Right.Object);
        }

        [Fact]
        public void ImagesAreUnsure_WhenEqualOnAllCounts()
        {
            SetupMock(mock: Right, size: 30000, date: DaysAgo(1), pixels: 1000);

            Entries.Add(new Pair(Good.Object, Right.Object));

            _sut.Compare(Entries);

            _sut.Unsure.Should().Contain(Good.Object);
            _sut.Unsure.Should().Contain(Right.Object);
        }
    }
}
