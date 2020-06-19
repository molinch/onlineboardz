using FluentAssertions;
using FluentAssertions.Equivalency;
using System;

namespace ApiTests.Persistence
{
    public static class FluentAssertionsMongoExtensions
    {
        public static EquivalencyAssertionOptions<TExpectation> WithMongoDateTime<TExpectation>(this EquivalencyAssertionOptions<TExpectation> options)
        {
            return options
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1000)) // Mongo slightly changes the datetime
                .WhenTypeIs<DateTime>();
        }
    }
}
