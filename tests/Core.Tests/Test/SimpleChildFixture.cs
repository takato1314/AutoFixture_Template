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
    }


    public class SimpleChildFixtureTest
    {
        [Theory, AutoMoqData]
        public Task CreateFixtures_TestEquivalency(IFixture fixture)
        {
            // Arrange
            var sut = new SimpleChildFixture(fixture);

            // Act
            var i0 = new SimpleChild();
            var i1 = sut.Object;
            var i2 = fixture.Create<SimpleChild>();
            var i3 = fixture.Create<IHasProperties>();

            // Assert
            i1.Should().NotBeNull();
            i0.Should().NotBeEquivalentTo(i1);
            i0.Should().NotBeEquivalentTo(i2);
            i0.Should().NotBeEquivalentTo(i3);
            i1.Should().NotBeEquivalentTo(i3);
            i1.Should().NotBeEquivalentTo(i2);
            i2.Should().NotBeEquivalentTo(i3);

            Mock.Get(i1).Should().NotBeNull();
            Mock.Get(i2).Should().NotBeNull();
            i2.IsMock().Should().BeTrue();
            i3.IsMock().Should().BeTrue();

            return Task.CompletedTask;
        }
        
        [Theory, AutoMoqData]
        public Task GetObject_ShouldReturnDifferentObjects(IFixture fixture)
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
                instance.Should().NotBeEquivalentTo(i1);

                instance.Name.Should().NotBe(i1.Name);
                instance.Number.Should().NotBe(i1.Number);
                instance.ConcurrencyStamp.Should().NotBe(i1.ConcurrencyStamp);
            }

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
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
            oldObject.Should().NotBeNull();
            newObject.Should().NotBeNull();
            newObject.IsMock().Should().BeFalse();
            newObject.Should().BeOfType<SimpleChild>();
            newObject.Should().NotBeEquivalentTo(oldObject);
            newObject.Should().BeSameAs(injected);
            newObject.Name.Should().Be("OverridenText");
            newObject.Number.Should().Be(111);
            newObject.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");

            return Task.CompletedTask;
        }

        [Theory, AutoMoqData]
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
            oldObject.Should().NotBeNull();
            newObject.Should().NotBeNull();
            newObject.IsMock().Should().BeFalse();
            newObject.Should().BeOfType<SimpleChild>();
            newObject.Should().NotBeEquivalentTo(oldObject);
            newObject.Should().BeSameAs(injected);
            newObject.Name.Should().Be("OverridenText");
            newObject.Number.Should().Be(111);
            newObject.ConcurrencyStamp.ToString().Should().Be("6f55a677-c447-45f0-8e71-95c7b73fa889");

            return Task.CompletedTask;
        }
    }
}