using System;
using FluentAssertions;
using Xunit;

namespace AutoFixture.Extensions.Tests
{
    /// <summary>
    /// More <see cref="IFixtureSetup"/> tests are available in implementation classes.
    /// </summary>
    public class BaseFixtureSetupTest
    {
        [Theory, AutoDomainData]
        public void TestFixtureSetup_ShouldHaveSameIFixtureInstance(IFixture fixture)
        {
            // Arrange
            var fixtureParent = new HasPropertiesDependentFixture(fixture);
            var oldFixture = fixtureParent.Dependencies.Fixture;

            // Act & Assert
            // One way to customize
            fixtureParent.Customize(fixture);
            var newFixture = fixtureParent.Dependencies.Fixture;
            Assert.Same(oldFixture, newFixture);

            // Another way to customize
            fixture.Customize(fixtureParent);
            newFixture = fixtureParent.Dependencies.Fixture;
            Assert.Same(oldFixture, newFixture);

            // One way to inject
            fixtureParent.Inject(fixture, new HasPropertiesSimple());
            newFixture = fixtureParent.Dependencies.Fixture;
            Assert.Same(oldFixture, newFixture);

            // Another way to inject
            fixture.Inject(fixtureParent, new HasPropertiesSimple());
            newFixture = fixtureParent.Dependencies.Fixture;
            Assert.Same(oldFixture, newFixture);
        }

        [Theory, AutoDomainData]
        public void NoFixtureAdded_ShouldThrowArgumentNullException(IFixture fixture)
        {
            // Arrange
            var fixtureParent = new HasPropertiesDependentFixture(fixture);

            // Act & Assert
            fixtureParent.Invoking(_ => _.Customize(null!)).Should().Throw<ArgumentNullException>();
            fixtureParent.Invoking(_ => _.Inject(null!, new HasPropertiesSimple())).Should().Throw<ArgumentNullException>();
        }
    }
}
