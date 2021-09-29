using System;
using AutoFixture.Xunit2;
using FluentAssertions;

namespace AutoFixture.Extensions
{
    /// <summary>
    /// A AutoFixture-xUnit attribute used for decorating unit test Theories.
    /// See https://blog.ploeh.dk/2011/03/18/EncapsulatingAutoFixtureCustomizations/
    /// </summary>
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        /// <inheritdoc cref="AutoMoqDataAttribute"/>
        public AutoMoqDataAttribute() : base(FixtureFactory.CreateFixture)
        {
            lock (FixtureLock)
            {
                // Setup fluent assertion
                SetupAssertionOptions();
            }
        }

        #region Properties

        private static bool _fluentOptionsSet;
        private static readonly object FixtureLock = new();

        #endregion

        /// <summary>
        /// Configures the Fluent Assertions options globally
        /// </summary>
        private static void SetupAssertionOptions()
        {
            if (!_fluentOptionsSet)
            {
                AssertionOptions.AssertEquivalencyUsing(options =>
                {
                    options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(5))).WhenTypeIs<DateTime>();
                    options.Using<DateTimeOffset>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(5))).WhenTypeIs<DateTimeOffset>();
                    options.Using(new EntityDtoSelectionRule());
                    _fluentOptionsSet = true;

                    return options;
                });
            }
        }
    }
}
