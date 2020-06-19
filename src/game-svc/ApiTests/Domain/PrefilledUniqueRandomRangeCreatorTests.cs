using Api.Domain;
using FluentAssertions;
using System.Diagnostics;
using Xunit;

namespace ApiTests.Domain
{
    public class PrefilledUniqueRandomRangeCreatorTests
    {
        private readonly UniqueRandomRangeCreator _creator;
        private readonly PrefilledUniqueRandomRangeCreator _prefilledCreator;

        public PrefilledUniqueRandomRangeCreatorTests()
        {
            _creator = new UniqueRandomRangeCreator();
            _prefilledCreator = new PrefilledUniqueRandomRangeCreator(new UniqueRandomRangeCreator());
        }

        [Fact]
        public void Should_generate_range()
        {
            // Act
            var range = _prefilledCreator.CreateArrayWithAllNumbersFromRange(5);

            range.Should().BeEquivalentTo(new[] { 0, 1, 2, 3, 4 });
        }

        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(10)]
        public void Should_be_faster(int rangeMax)
        {
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < 1000; i++)
            {
                _creator.CreateArrayWithAllNumbersFromRange(rangeMax);
            }
            var elapsedCreator = watch.Elapsed;

            watch = Stopwatch.StartNew();
            for (var i = 0; i < 1000; i++)
            {
                _prefilledCreator.CreateArrayWithAllNumbersFromRange(rangeMax);
            }
            var elapsedPrefilledCreator = watch.Elapsed;

            elapsedPrefilledCreator.Should().BeLessThan(elapsedCreator);
        }
    }
}
