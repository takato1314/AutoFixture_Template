using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoFixture.Extensions.Tests
{
    public class HasPropertiesSimpleFixture : BaseFixtureSetup<HasPropertiesSimple>
    {
        /// <inheritdoc />
        public HasPropertiesSimpleFixture(IFixture fixture) : base(fixture)
        {
            fixture.Customize(this);
        }

        protected override HasPropertiesSimple CreateObject(IFixture fixture)
        {
            // Mock is useful especially for scenario that uses HttpClients for connection or has heavy operations.
            // Otherwise, use actual class whenever possible.
            var hasProperties = new HasPropertiesSimple();
            return hasProperties;
        }

    }


    public class HasPropertiesSimpleFixtureTest
    {
        /// <summary>
        /// One way to setup your fixture.    
        /// The idea here is to enforce the practice that you write basic test to ensure that
        /// your fixture behaves correctly.
        /// </summary>
        [Theory, AutoDomainData]
        public Task TestCustomize_ShouldReturnMockObjects(IFixture fixture)
        {
            // Arrange
            var hasPropertiesFixture = new HasPropertiesSimpleFixture(fixture);
            
            // Act
            var i1 = hasPropertiesFixture.Object;
            var i2 = fixture.Create<HasPropertiesSimple>();
            var i3 = fixture.Create<IHasProperties>();

            // Assert
            i1.Should().NotBeNull();
            i1.Should().NotBeEquivalentTo(i2);
            i1.Should().NotBeEquivalentTo(i3);
            i2.Should().NotBeEquivalentTo(i3);

            return Task.CompletedTask;
        }
        
        [Theory, AutoDomainData]
        public Task TestGetDependencies_ShouldHaveOnlyOne(IFixture fixture)
        {
            // Arrange
            var sut = new HasPropertiesSimpleFixture(fixture);

            // Assert
            sut.Dependencies.FixtureDictionary.Should().HaveCount(1);
            sut.Dependencies.FixtureDictionary.Should().ContainKey(typeof(HasPropertiesSimple));
            sut.Object.Should().BeSameAs(sut.Dependencies[typeof(HasPropertiesSimple)].Object);
            sut.Object.Should().BeSameAs(sut.Dependencies.Get<HasPropertiesSimple>().Object);

            return Task.CompletedTask;
        }

        [Theory, AutoDomainData]
        public Task TestGetObject_ShouldReturnDefaultValues(IFixture fixture)
        {
            // Arrange
            var sut = new HasPropertiesSimpleFixture(fixture);

            // Assert
            // Object has default fixture implementation
            sut.Object.Should().NotBeNull();
            sut.Object.Text.Should().Be(string.Empty);
            sut.Object.Number.Should().Be(default);
            sut.Object.ConcurrencyStamp.Should().Be(default(Guid));

            var hasPropertiesList = new List<IHasProperties>
            {
                fixture.Create<IHasProperties>(),
                fixture.Create<HasPropertiesSimple>(),
            };
            foreach (var instance in hasPropertiesList)
            {
                // fixture.Create() function is not overriden in FixtureSetup
                Assert.NotNull(instance);
                instance.Text.Should().NotBe(sut.Object.Text);
                instance.Number.Should().NotBe(sut.Object.Number);
                instance.ConcurrencyStamp.Should().NotBe(sut.Object.ConcurrencyStamp);
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
            var sut = new HasPropertiesSimpleFixture(fixture);
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

            return Task.CompletedTask;
        }

        [Theory, AutoDomainData]
        public Task TestInject_AnotherWay_ShouldReturnOverwrittenValues(IFixture fixture)
        {
            // Arrange
            var mock = new Mock<HasPropertiesSimple>();
            mock.SetupProperty(_ => _.Text, "OverridenText");
            mock.SetupProperty(_ => _.Number, 111);
            mock.SetupProperty(_ => _.ConcurrencyStamp, new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889"));
            var sut = new HasPropertiesSimpleFixture(fixture);
            var oldObject = sut.Object;

            // Act
            fixture.Inject(sut, mock.Object);
            var newObject = sut.Dependencies[typeof(HasPropertiesSimple)].Object;

            // Assert
            Assert.NotNull(oldObject);
            Assert.NotNull(newObject);
            Assert.NotEqual(oldObject, newObject);
            Assert.Equal<HasPropertiesSimple>(mock.Object, newObject);
            Assert.Equal("OverridenText", newObject.Text);
            Assert.Equal<int>(111, newObject.Number);
            Assert.Equal("6f55a677-c447-45f0-8e71-95c7b73fa889", newObject.ConcurrencyStamp.ToString());

            return Task.CompletedTask;
        }
    }
}