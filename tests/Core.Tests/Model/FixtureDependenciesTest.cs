using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace AutoFixture.Extensions.Tests
{
    public class FixtureDependenciesTest
    {
        [Theory, AutoDomainData]
        public void TestRegister_ShouldStoreDependency(IFixture fixture)
        {
            // Arrange
            var fixtureParent = new HasPropertiesDependentFixture(fixture);
            var fixtureChild = new HasPropertiesSimpleFixture(fixture);
            var fixtureDependency = new FixtureDependencies(fixtureParent, fixture);
            fixtureDependency.FixtureDictionary.Should().BeEmpty();

            // Act
            fixtureDependency.Register(fixtureChild);

            // Assert
            fixtureDependency.FixtureDictionary.Should().NotBeEmpty();
            var result1 = fixtureDependency[typeof(HasPropertiesSimple)];
            var result2 = fixtureDependency.Get<HasPropertiesSimple>();
            var result3 = fixtureDependency.Get(typeof(HasPropertiesSimple));

            result1.Should().BeSameAs(fixtureChild);
            Assert.Same(fixtureChild, result2);
            Assert.Same(fixtureChild, result3);
        }

        [Theory, AutoDomainData]
        public void TestRegister_NonFixtureSetup_ShouldThrowException(IFixture fixture)
        {
            // Arrange
            var fixtureParent = new HasPropertiesDependentFixture(fixture);
            var fixtureDependency = new FixtureDependencies(fixtureParent, fixture);
            fixtureDependency.FixtureDictionary.Should().BeEmpty();

            // Act
            var action = fixtureDependency.Invoking(_ => _.Register(null!));

            // Assert
            action.Should().Throw<ArgumentException>();

        }

        [Theory, AutoDomainData]
        public void TestUpdate_ShouldUpdateDependency(IFixture fixture)
        {
            // Arrange
            var fixtureParent = new HasPropertiesDependentFixture(fixture);
            var fixtureChild = new HasPropertiesSimpleFixture(fixture);
            var fixtureDependency = new FixtureDependencies(fixtureParent, fixture);
            fixtureDependency.FixtureDictionary.Should().BeEmpty();
            fixtureDependency.Register(fixtureChild);

            var oldObject = fixtureChild.Object;
            var newObject = fixture.Create<HasPropertiesSimple>();

            // Act
            fixtureDependency.Update(newObject);

            // Assert
            newObject.Should().NotBeEquivalentTo(oldObject);
            var result = fixtureDependency[typeof(HasPropertiesSimple)].Object;
            Assert.Same(result, newObject);
            var created = fixture.Create<HasPropertiesSimple>();
            Assert.Same(newObject, created);
        }

        [Theory, AutoDomainData]
        public void TestUpdate_NonDependency_ShouldThrowException(IFixture fixture)
        {
            // Arrange
            var fixtureParent = new HasPropertiesDependentFixture(fixture);
            var fixtureChild = new HasPropertiesSimpleFixture(fixture);
            var fixtureDependency = new FixtureDependencies(fixtureParent, fixture);
            fixtureDependency.FixtureDictionary.Should().BeEmpty();
            fixtureDependency.Register(fixtureChild);

            // Act
            var action = fixtureDependency.Invoking(_ => _.Update(new HasPropertiesComplex()));

            // Assert
            action.Should().Throw<DirectoryNotFoundException>();
        }
    }
}
