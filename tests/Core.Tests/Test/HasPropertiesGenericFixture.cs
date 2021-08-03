using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoFixture.Extensions.Tests
{
    public class HasPropertiesGenericFixture<T> : BaseFixtureSetup<HasPropertiesGeneric<T>> where T : IHasProperties, new()
    {
        /// <inheritdoc />
        public HasPropertiesGenericFixture(IFixture fixture) : base(fixture)
        {
        }

        /// <inheritdoc />
        protected override void Register(IFixture fixture)
        {
            // Add dependencies related for the current fixture.
            Dependencies.Register(new HasPropertiesSimpleFixture(fixture));
            Dependencies.Register(new HasPropertiesComplexFixture(fixture));
            Dependencies.Register(new HasPropertiesDependentFixture(fixture));

            // Override default creation function for fixture.Create()
            fixture.Register<IFixture, HasPropertiesGeneric<T>>(CreateObject);
            fixture.Register<IFixture, IHasProperties>(CreateObject);
        }

        protected override HasPropertiesGeneric<T> CreateObject(IFixture fixture)
        {
            // Mock is useful especially for scenario that uses HttpClients for connection or has heavy operations.
            // Otherwise, use actual class whenever possible.
            var genericValue = fixture.Create<T>();
            var mock = new Mock<HasPropertiesGeneric<T>> { CallBase = true };
            mock.SetupProperty(_ => _.GenericValue, genericValue);
            var hasPropertiesGeneric = mock.Object;

            return hasPropertiesGeneric;
        }
    }

    public class HasPropertiesGenericFixtureTest
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
            var hasPropertiesFixture = new HasPropertiesGenericFixture<HasPropertiesComplex>(fixture);

            // Act
            hasPropertiesFixture.Customize(fixture);

            // Assert
            var i1 = hasPropertiesFixture.Object;
            var i2 = fixture.Create<HasPropertiesGeneric<HasPropertiesComplex>>();
            var i3 = fixture.Create<IHasProperties>();
            Assert.NotNull(i1);
            Assert.NotNull(Mock.Get(i1));
            i2.Should().NotBeSameAs(i1);
            i2.Should().NotBeSameAs(i3);
            i3.Should().NotBeSameAs(i1);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Another way to setup your fixture using AutoFixture <see cref="IFixture"/>.
        /// </summary>
        [Theory, AutoDomainData]
        public Task TestCustomize_AnotherWay_ReturnsMockObject(IFixture fixture)
        {
            // Arrange
            var hasPropertiesFixture = new HasPropertiesGenericFixture<HasPropertiesComplex>(fixture);

            // Act
            fixture.Customize(hasPropertiesFixture);

            // Assert
            var i1 = hasPropertiesFixture.Object;
            var i2 = fixture.Create<HasPropertiesGeneric<HasPropertiesComplex>>();
            var i3 = fixture.Create<IHasProperties>();
            Assert.NotNull(i1);
            Assert.NotNull(Mock.Get(i1));
            i2.Should().NotBeSameAs(i1);
            i2.Should().NotBeSameAs(i3);
            i3.Should().NotBeSameAs(i1);

            return Task.CompletedTask;
        }

        [Theory, AutoDomainData]
        public Task TestDependency_ShouldHaveOnlyOne(IFixture fixture)
        {
            // Arrange
            var sut = new HasPropertiesGenericFixture<HasPropertiesComplex>(fixture) as IFixtureSetup;

            // Act
            fixture.Customize(sut);

            // Assert
            sut.Dependencies.FixtureDictionary.Should().ContainKey(typeof(HasPropertiesGeneric<HasPropertiesComplex>));
            sut.Dependencies.FixtureDictionary.Should().ContainKey(typeof(HasPropertiesDependent));
            sut.Dependencies.FixtureDictionary.Should().ContainKey(typeof(HasPropertiesSimple));
            sut.Dependencies.FixtureDictionary.Should().ContainKey(typeof(HasPropertiesComplex));
            Assert.Same(sut.Object, sut.Dependencies[typeof(HasPropertiesGeneric<HasPropertiesComplex>)].Object);
            return Task.CompletedTask;
        }

        [Theory, AutoDomainData]
        public Task TestGetObject_ShouldReturnDefaultValues(IFixture fixture)
        {
            // Arrange
            var sut = new HasPropertiesGenericFixture<HasPropertiesComplex>(fixture);
            var fixture2 = new HasPropertiesComplexFixture(fixture);
            fixture2.Customize(fixture);
            var hasPropertiesComplexExpected = fixture2.Object;

            // Act
            fixture.Customize(sut);

            // Assert
            // Object has default fixture implementation
            Assert.NotNull(sut.Object);
            Assert.Equal("DefaultGenericString", sut.Object.Text);
            Assert.Equal(default(int), sut.Object.Number);
            Assert.Equal(default(Guid), sut.Object.ConcurrencyStamp);
            ((HasPropertiesComplex) hasPropertiesComplexExpected).Should()
                .BeEquivalentTo((HasPropertiesComplex) sut.Object.GenericValue);
            Assert.Equal(sut.Object.Number.ToString(), sut.Object.GetValue());

            var hasPropertiesList = new List<IHasProperties>
            {
                fixture.Create<IHasProperties>(),
                fixture.Create<HasPropertiesGeneric<HasPropertiesComplex>>(),
            };

            foreach (var instance in hasPropertiesList)
            {
                // fixture.Register() function is overriden in FixtureSetup
                Assert.NotNull(instance);
                instance.Should().BeEquivalentTo((HasPropertiesGeneric<HasPropertiesComplex>)sut.Dependencies[typeof(HasPropertiesGeneric<HasPropertiesComplex>)].Object);
                instance.Should().BeEquivalentTo((HasPropertiesGeneric<HasPropertiesComplex>)sut.Object);
            }

            return Task.CompletedTask;
        }

        [Theory, AutoDomainData]
        public Task TestInject_ShouldReturnOverwrittenValues(IFixture fixture)
        {
            // Arrange
            var mock = new Mock<HasPropertiesComplex>();
            mock.SetupProperty(_ => _.Text, "OverridenText");
            mock.SetupProperty(_ => _.Number, 111);
            mock.SetupProperty(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"));
            mock.Setup(_ => _.GetValue()).Returns("No longer throws exception");
            var sut = new HasPropertiesComplexFixture(fixture);
            sut.Customize(fixture);
            var oldObject = sut.Object;

            // Act
            sut.Inject(fixture, mock.Object);
            var newObject = sut.Object;

            // Assert
            oldObject.Should().NotBeNull();
            newObject.Should().NotBeNull();
            newObject.Should().NotBeEquivalentTo(oldObject);
            newObject.Should().BeEquivalentTo(mock.Object);
            newObject.Text.Should().Be("OverridenText");
            newObject.Number.Should().Be(111);
            newObject.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");
            oldObject.Invoking(_ => _.GetValue())
                .Should()
                .Throw<NotImplementedException>()
                .WithMessage("Not implemented on class");
            newObject.GetValue().Should().Be("No longer throws exception");

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
            var sut = new HasPropertiesComplexFixture(fixture);
            fixture.Customize(sut);
            var oldObject = sut.Object;

            // Act
            fixture.Inject(sut, mock.Object);
            var newObject = sut.Dependencies[typeof(HasPropertiesComplex)].Object;

            // Assert
            Assert.NotNull(oldObject);
            Assert.NotNull(newObject);
            Assert.NotEqual(oldObject, newObject);
            Assert.Equal<HasPropertiesComplex>(mock.Object, newObject);
            Assert.Equal("OverridenText", newObject.Text);
            Assert.Equal<int>(111, newObject.Number);
            Assert.Equal("6f55a677-c447-45f0-8e71-95c7b73fa889", newObject.ConcurrencyStamp.ToString());
            Assert.Throws<NotImplementedException>(() => oldObject.GetValue());
            Assert.Equal("No longer throws exception", newObject.GetValue());

            return Task.CompletedTask;
        }
    }
}
