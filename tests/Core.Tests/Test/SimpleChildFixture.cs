using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoFixture.Extensions.Tests
{
    public class SimpleChildFixture : BaseFixtureSetup<SimpleChild>
    {
        /// <inheritdoc />
        public SimpleChildFixture(IFixture fixture) : base(fixture)
        {
        }
        
        /// <inheritdoc />
        protected override void Register(IFixture fixture)
        {
            // Override default creation function above for these instances.
            fixture.Register<IFixture, IHasProperties>(CreateObject);
        }
    }


    public class SimpleChildFixtureTest
    {
        [Theory, AutoDomainData]
        public Task CreateFixtures_TestEquivalency(IFixture fixture)
        {
            // Arrange
            var sut = new SimpleChildFixture(fixture);
            
            // Act
            var i1 = sut.Object;
            var i2 = fixture.Create<SimpleChild>();
            var i3 = fixture.Create<IHasProperties>();

            // Assert
            i1.Should().NotBeNull();
            Mock.Get(i1).Should().NotBeNull();
            i1.Should().NotBeEquivalentTo(i2);
            i1.Should().NotBeEquivalentTo(i3);
            i2.Should().NotBeEquivalentTo(i3);

            var actualType = i1.GetType();
            i2.Should().BeOfType(actualType);
            i3.Should().BeOfType(actualType);

            return Task.CompletedTask;
        }
        
        [Theory, AutoDomainData]
        public Task GetObject_ShouldReturnObjects(IFixture fixture)
        {
            // Arrange
            var sut = new SimpleChildFixture(fixture);

            // Act
            var i1 = sut.Object;

            // Assert
            i1.Should().NotBeNull();
            Mock.Get(i1).Should().NotBeNull();
            i1.Name.Should().NotBeNullOrEmpty();
            i1.Number.Should().NotBe(default);
            i1.ConcurrencyStamp.Should().NotBe(default);

            var hasPropertiesList = new List<IHasProperties>
            {
                fixture.Create<IHasProperties>(),
                fixture.Create<SimpleChild>(),
            };
            foreach (var instance in hasPropertiesList)
            {
                instance.Should().NotBeNull();
                instance.Name.Should().NotBe(i1.Name);
                instance.Number.Should().NotBe(i1.Number);
                instance.ConcurrencyStamp.Should().NotBe(i1.ConcurrencyStamp);
            }

            return Task.CompletedTask;
        }

        [Theory, AutoDomainData]
        public Task TestInject_ShouldReturnInjectedObject(IFixture fixture)
        {
            // Arrange
            var injected = new SimpleChild("OverridenText", 111)
            {
                ConcurrencyStamp = new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889")
            };
            var sut = new SimpleChildFixture(fixture);
            var oldObject = sut.Object;

            // Act
            sut.Inject(fixture, injected);

            // Assert
            var newObject = sut.Object;
            Assert.Throws<ArgumentException>(() => Mock.Get(newObject));
            oldObject.Should().NotBeNull();
            newObject.Should().NotBeNull();
            newObject.Should().NotBeEquivalentTo(oldObject);
            newObject.Should().BeEquivalentTo(injected);
            newObject.Name.Should().Be("OverridenText");
            newObject.Number.Should().Be(111);
            newObject.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");

            return Task.CompletedTask;
        }

        [Theory, AutoDomainData]
        public Task TestInject_AnotherWay_ShouldReturnInjectedObject(IFixture fixture)
        {
            // Arrange
            var injected = new SimpleChild("OverridenText", 111)
            {
                ConcurrencyStamp = new Guid("6f55a677-c447-45f0-8e71-95c7b73fa889")
            };
            var sut = new SimpleChildFixture(fixture);
            var oldObject = sut.Object;

            // Act
            fixture.Inject(sut, injected);

            // Assert
            var newObject = sut.Object;
            Assert.Throws<ArgumentException>(() => Mock.Get(newObject));
            oldObject.Should().NotBeNull();
            newObject.Should().NotBeNull();
            newObject.Should().NotBeEquivalentTo(oldObject);
            newObject.Should().BeSameAs(injected);
            newObject.Name.Should().Be("OverridenText");
            newObject.Number.Should().Be(111);
            newObject.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");

            return Task.CompletedTask;
        }
    }
}