using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoFixture.Extensions.Tests
{
    public class HasPropertiesNestedDependentFixture : BaseFixtureSetup<HasPropertiesNestedDependent>
    {
        /// <inheritdoc />
        public HasPropertiesNestedDependentFixture(IFixture fixture) : base(fixture)
        {
        }

        /// <inheritdoc />
        protected override void Register(IFixture fixture)
        {
            // Add dependencies related for the current fixture.
            Dependencies.Register(new HasPropertiesDependentFixture(fixture));

            // Override default creation function for fixture.Create()
            fixture.Register<IFixture, HasPropertiesNestedDependent>(CreateObject);
        }

        #region Private

        protected override HasPropertiesNestedDependent CreateObject(IFixture fixture)
        {
            // Mock is useful especially for scenario that uses HttpClients for connection or has heavy operations.
            // Otherwise, use actual class whenever possible.
            var hasPropertiesNestedDependent = new HasPropertiesNestedDependent
            {
                // Create a new instance with injected value
                HasPropertiesDependent1 = fixture.Create<HasPropertiesDependent>(),

                // Get static instance of fixture
                HasPropertiesDependent2 = Dependencies.Get<HasPropertiesDependent>().Object
            };

            return hasPropertiesNestedDependent;
        }

        #endregion

    }

    public class HasPropertiesNestedDependentFixtureTest
    {
        /// <summary>
        /// One way to setup your fixture.    
        /// The idea here is to enforce the practice that you write basic test to ensure that
        /// your fixture behaves correctly.
        /// </summary>
        [Theory, AutoDomainData]
        public Task TestCustomize_ReturnsMockObject(IFixture fixture)
        {
            // Arrange
            var hasPropertiesFixture = new HasPropertiesNestedDependentFixture(fixture);

            // Act
            hasPropertiesFixture.Customize(fixture);

            // Assert
            var i1 = hasPropertiesFixture.Object;
            var i2 = fixture.Create<HasPropertiesNestedDependent>();
            Assert.NotNull(i1);
            Assert.Throws<ArgumentException>(() => Mock.Get(i1));
            i2.Should().NotBeSameAs(i1);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Another way to setup your fixture using AutoFixture <see cref="IFixture"/>.
        /// </summary>
        [Theory, AutoDomainData]
        public Task TestCustomize_AnotherWay_ReturnsMockObject(IFixture fixture)
        {
            // Arrange
            var hasPropertiesFixture = new HasPropertiesNestedDependentFixture(fixture);

            // Act
            fixture.Customize(hasPropertiesFixture);

            // Assert
            var i1 = hasPropertiesFixture.Object;
            var i2 = fixture.Create<HasPropertiesNestedDependent>();
            Assert.NotNull(i1);
            Assert.Throws<ArgumentException>(() => Mock.Get(i1));
            i2.Should().NotBeSameAs(i1);

            return Task.CompletedTask;
        }

        [Theory, AutoDomainData]
        public Task TestDependency_ShouldHaveOnlyOne(IFixture fixture)
        {
            // Arrange
            var sut = new HasPropertiesNestedDependentFixture(fixture) as IFixtureSetup;

            // Act
            fixture.Customize(sut);

            // Assert
            sut.Dependencies.FixtureDictionary.Should().ContainKey(typeof(HasPropertiesNestedDependent));
            sut.Dependencies.FixtureDictionary.Should().ContainKey(typeof(HasPropertiesDependent));
            sut.Dependencies.FixtureDictionary.Should().ContainKey(typeof(HasPropertiesSimple));
            sut.Dependencies.FixtureDictionary.Should().ContainKey(typeof(HasPropertiesComplex));
            Assert.Same(sut.Object, sut.Dependencies[typeof(HasPropertiesNestedDependent)].Object);
            return Task.CompletedTask;
        }

        [Theory, AutoDomainData]
        public Task TestGetObject_ShouldReturnDefaultValues(IFixture fixture)
        {
            // Arrange
            var sut = new HasPropertiesNestedDependentFixture(fixture);
            var fixture1 = new HasPropertiesSimpleFixture(fixture);
            fixture1.Customize(fixture);
            var hasPropertiesSimpleExpected = fixture1.Object;
            var fixture2 = new HasPropertiesComplexFixture(fixture);
            fixture2.Customize(fixture);
            var hasPropertiesComplexExpected = fixture2.Object;

            // Act
            fixture.Customize(sut);

            // Assert
            // Object has default fixture implementation
            Assert.NotNull(sut.Object);
            sut.Object.HasPropertiesDependent1.HasPropertiesSimple.Should()
                .BeEquivalentTo(
                    hasPropertiesSimpleExpected);
            sut.Object.HasPropertiesDependent1.HasPropertiesComplex.Should()
                .BeEquivalentTo(
                    hasPropertiesComplexExpected);
            sut.Object.HasPropertiesDependent2.HasPropertiesSimple.Should()
                .BeEquivalentTo(
                    hasPropertiesSimpleExpected);
            sut.Object.HasPropertiesDependent2.HasPropertiesComplex.Should()
                .BeEquivalentTo(
                    hasPropertiesComplexExpected);

            var hasPropertiesList = new List<HasPropertiesNestedDependent>
            {
                fixture.Create<HasPropertiesNestedDependent>(),
            };

            foreach (var instance in hasPropertiesList)
            {
                // fixture.Register() function is overriden in FixtureSetup
                Assert.NotNull(instance);
                instance.Should().BeEquivalentTo(sut.Object);
            }

            return Task.CompletedTask;
        }

        [Theory, AutoDomainData]
        public Task TestInject_ShouldReturnOverwrittenValues(IFixture fixture)
        {
            // Arrange
            var mock = new Mock<HasPropertiesSimple>();
            mock.SetupProperty(_ => _.Text, "OverridenText");
            mock.SetupProperty(_ => _.Number, 111);
            mock.SetupProperty(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"));

            var sut = new HasPropertiesNestedDependentFixture(fixture);
            sut.Customize(fixture);
            var oldObject = sut.Object;
            var oldHasPropertiesSimple = sut.Dependencies[typeof(HasPropertiesSimple)].Object as HasPropertiesSimple;
            var oldHasPropertiesComplex = sut.Dependencies[typeof(HasPropertiesComplex)].Object as HasPropertiesComplex;
            oldObject.HasPropertiesDependent1.HasPropertiesSimple.Should().BeEquivalentTo(oldHasPropertiesSimple);
            oldObject.HasPropertiesDependent1.HasPropertiesComplex.Should().BeEquivalentTo(oldHasPropertiesComplex);
            oldObject.HasPropertiesDependent2.HasPropertiesSimple.Should().BeEquivalentTo(oldHasPropertiesSimple);
            oldObject.HasPropertiesDependent2.HasPropertiesComplex.Should().BeEquivalentTo(oldHasPropertiesComplex);

            // Act
            sut.Inject(fixture, mock.Object);
            var newObject = sut.Object;
            var newHasPropertiesSimple = sut.Dependencies[typeof(HasPropertiesSimple)].Object as HasPropertiesSimple;
            var newHasPropertiesComplex = sut.Dependencies[typeof(HasPropertiesComplex)].Object as HasPropertiesComplex;

            // Assert
            oldObject.Should().NotBeNull();
            newObject.Should().NotBeNull();
            newObject.Should().NotBeEquivalentTo(oldObject);
            newObject.Should().BeEquivalentTo(sut.Dependencies[typeof(HasPropertiesNestedDependent)].Object);

            newHasPropertiesSimple.Should().NotBeEquivalentTo(oldHasPropertiesSimple);
            newHasPropertiesSimple.Should().BeEquivalentTo(mock.Object);
            newHasPropertiesComplex.Should().BeEquivalentTo(oldHasPropertiesComplex);
            newHasPropertiesSimple.Should().BeEquivalentTo(newObject.HasPropertiesDependent1.HasPropertiesSimple);
            newHasPropertiesComplex.Should().BeEquivalentTo(newObject.HasPropertiesDependent1.HasPropertiesComplex);
            oldHasPropertiesSimple.Should().BeEquivalentTo(newObject.HasPropertiesDependent2.HasPropertiesSimple);
            oldHasPropertiesComplex.Should().BeEquivalentTo(newObject.HasPropertiesDependent2.HasPropertiesComplex);

            return Task.CompletedTask;
        }

        [Theory, AutoDomainData]
        public Task TestInject_AnotherWay_ShouldReturnOverwrittenValues(IFixture fixture)
        {
            // Arrange
            var mock = new Mock<HasPropertiesComplex>();
            mock.SetupProperty(_ => _.Text, "OverridenText");
            mock.SetupProperty(_ => _.Number, 111);
            mock.SetupProperty(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"));
            mock.Setup(_ => _.GetValue()).Returns("No longer throws exception");
            
            var sut = new HasPropertiesNestedDependentFixture(fixture);
            fixture.Customize(sut);
            var oldObject = sut.Object;
            var oldHasPropertiesSimple = sut.Dependencies[typeof(HasPropertiesSimple)].Object as HasPropertiesSimple;
            var oldHasPropertiesComplex = sut.Dependencies[typeof(HasPropertiesComplex)].Object as HasPropertiesComplex;
            oldObject.HasPropertiesDependent1.HasPropertiesSimple.Should().BeEquivalentTo(oldHasPropertiesSimple);
            oldObject.HasPropertiesDependent1.HasPropertiesComplex.Should().BeEquivalentTo(oldHasPropertiesComplex);
            oldObject.HasPropertiesDependent2.HasPropertiesSimple.Should().BeEquivalentTo(oldHasPropertiesSimple);
            oldObject.HasPropertiesDependent2.HasPropertiesComplex.Should().BeEquivalentTo(oldHasPropertiesComplex);

            // Act
            fixture.Inject(sut, mock.Object);
            var newObject = sut.Dependencies[typeof(HasPropertiesNestedDependent)].Object;
            var newHasPropertiesSimple = sut.Dependencies[typeof(HasPropertiesSimple)].Object as HasPropertiesSimple;
            var newHasPropertiesComplex = sut.Dependencies[typeof(HasPropertiesComplex)].Object as HasPropertiesComplex;

            // Assert
            Assert.NotNull(oldObject);
            Assert.NotNull(newObject);
            Assert.NotEqual(oldObject, newObject);
            Assert.Equal(mock.Object, newHasPropertiesComplex);

            newHasPropertiesSimple.Should().BeEquivalentTo(oldHasPropertiesSimple);
            newHasPropertiesComplex.Should().NotBeEquivalentTo(oldHasPropertiesSimple);
            newHasPropertiesComplex.Should().BeEquivalentTo(mock.Object);
            newHasPropertiesSimple.Should().BeEquivalentTo(newObject.HasPropertiesDependent1.HasPropertiesSimple);
            newHasPropertiesComplex.Should().BeEquivalentTo(newObject.HasPropertiesDependent1.HasPropertiesComplex);
            oldHasPropertiesSimple.Should().BeEquivalentTo(newObject.HasPropertiesDependent2.HasPropertiesSimple);
            //oldHasPropertiesComplex.Should().BeEquivalentTo(newObject.HasPropertiesDependent2.HasPropertiesComplex);

            return Task.CompletedTask;
        }

    }
}
