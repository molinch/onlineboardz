using Api.Domain;
using FluentAssertions;
using Xunit;

namespace ApiTests.Domain
{
    public class UniqueRandomRangeTests
    {
        [Fact]
        public void Should_generate_range()
        {
            var creator = new UniqueRandomRangeCreator();

            // Act
            var range = creator.CreateArrayWithAllNumbersFromRange(5);

            range.Should().BeEquivalentTo(new[] { 0, 1, 2, 3, 4 });
        }
    }
}
