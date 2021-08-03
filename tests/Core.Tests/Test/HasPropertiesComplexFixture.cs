using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoFixture.Extensions.Tests
{
    public class HasPropertiesComplexFixture : BaseFixtureSetup<HasPropertiesComplex>
    {
        /// <inheritdoc />
        public HasPropertiesComplexFixture(IFixture fixture) : base(fixture)
        {
        }
        
        /// <inheritdoc />
        protected override void Register(IFixture fixture)
        {
            // See https://github.com/AutoFixture/AutoFixture/issues/731
            fixture.Customize<HasPropertiesComplex>(
                composer =>
                {
                    var postProcess = composer
                        .Without(_ => _.Nullable);
                    return postProcess.Do(
                        item =>
                        {
                            item.Nullable = 100;
                        });
                });

            // Override default creation function above for these instances.
            fixture.Register<IFixture, IHasProperties>(CreateObject);
            fixture.Register<IFixture, HasPropertiesComplex>(CreateObject);

        }

        protected override HasPropertiesComplex CreateObject(IFixture fixture)
        {
            // Mock is useful especially for scenario that uses HttpClients for connection or has heavy operations.
            // Otherwise, use actual class whenever possible.
            var mock = new Mock<HasPropertiesComplex> {CallBase = true};
            var hasProperties = mock.Object;

            return hasProperties;
        }
    }


    public class HasPropertiesComplexFixtureTest
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
            var hasPropertiesFixture = new HasPropertiesComplexFixture(fixture);

            // Act
            hasPropertiesFixture.Customize(fixture);

            // Assert
            var i1 = hasPropertiesFixture.Object;
            var i2 = fixture.Create<HasPropertiesComplex>();
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
            var hasPropertiesFixture = new HasPropertiesComplexFixture(fixture);

            // Act
            fixture.Customize(hasPropertiesFixture);

            // Assert
            var i1 = hasPropertiesFixture.Object;
            var i2 = fixture.Create<HasPropertiesComplex>();
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
            var sut = new HasPropertiesComplexFixture(fixture) as IFixtureSetup;

            // Act
            fixture.Customize(sut);

            // Assert
            sut.Dependencies.FixtureDictionary.Should().ContainKey(typeof(HasPropertiesComplex));
            Assert.Same(sut.Object, sut.Dependencies[typeof(HasPropertiesComplex)].Object);
            return Task.CompletedTask;
        }

        [Theory, AutoDomainData]
        public Task TestGetObject_ShouldReturnDefaultValues(IFixture fixture)
        {
            // Arrange
            var sut = new HasPropertiesComplexFixture(fixture);

            // Act
            fixture.Customize(sut);

            // Assert
            // Object has default fixture implementation
            Assert.NotNull(sut.Object);
            Assert.Equal("DefaultString", sut.Object.Text);
            Assert.Equal(default(int), sut.Object.Number);
            Assert.Null(sut.Object.Nullable);
            Assert.Equal(default(Guid), sut.Object.ConcurrencyStamp);

            var hasPropertiesList = new List<IHasProperties>
            {
                fixture.Create<IHasProperties>(),
                fixture.Create<HasPropertiesComplex>(),
            };

            foreach (var instance in hasPropertiesList)
            {
                // fixture.Register() function is overriden in FixtureSetup
                Assert.NotNull(instance);
                instance.Text.Should().Be(sut.Object.Text);
                instance.Number.Should().Be(sut.Object.Number);
                instance.ConcurrencyStamp.Should().Be(sut.Object.ConcurrencyStamp);
                instance.Invoking(_ => _.GetValue())
                    .Should()
                    .Throw<NotImplementedException>();
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