using Moq;
using System.Collections.Generic;
using Xunit;

namespace DupFinderCore.Tests
{

    public class ImageComparerTests
    {
        private readonly ImageComparer _sut;

        private readonly ImageComparisonRuleset _rules = new ImageComparisonRuleset();

        private readonly UserSettings _settings = new UserSettings()
        { CompareByDate = true, CompareByPixels = true, CompareBySize = true };

        private readonly List<(IEntry, IEntry)> Images = new List<(IEntry, IEntry)>();

        public ImageComparerTests()
        {
            _sut = new ImageComparer(_rules);

            Images = CreateMockImages();
        }

        private List<(IEntry, IEntry)> CreateMockImages()
        {
            var list = new List<(IEntry, IEntry)>();

            var moq1 = new Mock<IEntry>();
            moq1.SetupGet(x => x.Size).Returns(40000);

            var moq2 = new Mock<IEntry>();
            moq2.SetupGet(x => x.Size).Returns(40000);


            //author.SetupGet(p => p.Id).Returns(1);
            //author.SetupGet(p => p.FirstName).Returns(“Joydip”);
            //author.SetupGet(p => p.LastName).Returns(“Kanjilal”);
            //Assert.AreEqual(“Joydip”, author.Object.FirstName);
            //Assert.AreEqual(“Kanjilal”, author.Object.LastName);

            list.Add((moq1.Object, moq2.Object));

            return Images;
        }

        [Fact]
        public void dotest()
        {
            _sut.Process(Images, _settings);

            Assert.True(true);
        }

    }
}
